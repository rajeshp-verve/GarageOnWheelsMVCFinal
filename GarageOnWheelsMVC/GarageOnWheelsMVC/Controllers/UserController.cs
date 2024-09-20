﻿using GarageOnWheelsMVC.Helper;
using GarageOnWheelsMVC.Models;
using GarageOnWheelsMVC.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using NuGet.Common;
using Microsoft.AspNetCore.Authorization;


namespace GarageOnWheelsMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiHelper _apiHelper;
        private string baseurl;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        public UserController(HttpClient httpClient, IConfiguration configuration, ApiHelper apiHelper)
        {
            _httpClient = httpClient;
            baseurl = configuration["AppSettings:BaseUrl"];
            _apiHelper = apiHelper;
        }
        public IActionResult Dashboard()
        {
            return View();
        }


        //Get All User
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _apiHelper.GetAsync<List<User>>("user/all");
            if (users == null)
            {
                return BadRequest("Error occurs during fetch user ");
            }
            return View(users);
        }

        //Get All Customer
        [Authorize(Roles = "GarageOwner")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var users = await _apiHelper.GetAsync<List<User>>("user/allCustomer");
            if (users == null)
            {
                return BadRequest("Error occurs during fetch Customers ");
            }
            return View(users);
        }
        //Ge all GarageOwner
        [Authorize(Roles = "SuperAdmin")]
        public async Task<JsonResult> GetAllGarageOwners()
        {
            var users = await _apiHelper.GetAsync<List<User>>("user/allgarageowner");
            return new JsonResult(users ?? new List<User>());
        }

        //Get Users By Role
        [Authorize(Roles = "SuperAdmin")]
        public async Task<JsonResult> GetUsersByRole()
        {
            var users = await _apiHelper.GetAsync<List<User>>("user/by-role?role=GarageOwner");
            return new JsonResult(users ?? new List<User>());
        }

       
        // Get Form to Create the User
        [Authorize(Roles = "SuperAdmin,GarageOwner")]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Handle New user Creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email exists
            if (await IsEmailExists(model.Email))
            {
                return View(model);
            }

            var userModel = RegisterViewModel.Mapping(model);
            userModel.CreatedBy = SessionHelper.GetUserIdFromToken(HttpContext);
            var response = await _apiHelper.SendPostRequest("user/create", userModel);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                await _apiHelper.SendOtp(model.Email);
                TempData["Email"] = model.Email;
                return RedirectToAction("VerifyOtp");
            }

            return View(model);
        }

        public IActionResult VerifyOtp()
        {
            TempData.Keep("Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(OtpVerificationViewModel model)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"auth/verify-email?email={model.Email}&otp={model.OTP}");
            var response = await _apiHelper.SendJsonAsync(request.RequestUri.ToString(), model, HttpMethod.Post);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Invalid OTP. Please try again.");
            TempData.Keep("Email");
            return View(model);
        }
        // Edit the User
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _apiHelper.GetAsync<User>($"user/{id}");
            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = RegisterViewModel.Mapping(user);
            return View(userViewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(RegisterViewModel model, string? previousUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.previousurl = Request.Headers["Referer"].ToString();
                return View(model);
            }

            model.UpdatedBy = SessionHelper.GetUserIdFromToken(HttpContext);
            var response = await _apiHelper.SendJsonAsync($"user/update/{model.Id}", model, HttpMethod.Put);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                TempData["Successful"] = "User successfully updated!";
                return !string.IsNullOrEmpty(previousUrl) ? Redirect(previousUrl) : RedirectToAction("GetAllUsers");
            }

            return View(model);
        }

        //Edit Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile(Guid id)
        {
            var user = await _apiHelper.GetAsync<User>($"user/{id}");
            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = RegisterViewModel.Mapping(user);
            return View(userViewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(UpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UpdatedBy = SessionHelper.GetUserIdFromToken(HttpContext);
            var response = await _apiHelper.SendJsonAsync($"user/update/{model.Id}", model, HttpMethod.Put);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return RedirectToAction("Dashboard", "Account");
            }

            return View(model);
        }


        public async Task<IActionResult> Delete(Guid id)
        {        
            var response = await _httpClient.DeleteAsync($"{baseurl}user/delete/{id}");
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                TempData["Successful"] = "User successfully deleted!";
                return RedirectToAction("GetAllUsers");
            }
            return BadRequest();
        }

        // Helper Method
       


        // Check the Email Exist or not
        private async Task<bool> IsEmailExists(string email)
        {
            var emailExists = await _apiHelper.CheckIfExists($"user/search?email={email}");
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email Already Exist.");
            }
            return emailExists;
        }

        //Change Password
        public IActionResult ChangePassword(Guid id)
        {
            ViewBag.id = id;
            return View();
        }
        
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.id = id;
                return View(model);
            }

            // Create a new change password request object
            var changePasswordRequest = new
            {
                currentPassword = model.OldPassword,
                newPassword = model.NewPassword
            };

            // Send the request using ApiHelper
            var response = await _apiHelper.SendJsonAsync($"user/change-password/{id}", changePasswordRequest, HttpMethod.Post);

            if (response.IsSuccessStatusCode)
            {
                TempData["Successful"] = "Password changed successfully.";
                return RedirectToAction("ChangePassword");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", message);
            }

            ViewBag.id = id;
            return View(model);
        }


    }
}