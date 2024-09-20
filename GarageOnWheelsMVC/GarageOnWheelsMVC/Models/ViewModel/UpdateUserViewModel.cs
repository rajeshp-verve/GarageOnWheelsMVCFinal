﻿using System.ComponentModel.DataAnnotations;

namespace GarageOnWheelsMVC.Models.ViewModel
{
    public class UpdateUserViewModel
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Select a Role")]
        public UserRole Role { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNo { get; set; }
        [Required(ErrorMessage = "Select a Gender")]
        public Gender Gender { get; set; } = Gender.Male;
        [Required]
        public string Address { get; set; }
        public bool IsEmailVerified { get; set; } = false;

        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [Required]
        public int CountryId { get; set; }
        [Required]
        public int StateId { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public int AreaId { get; set; }
        [Required]
        public bool IsDelete { get; set; } = false;


        public static User mapping(UpdateUserViewModel userDto)
        {
            return new User()
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Role = userDto.Role,
                Address = userDto.Address,
                PhoneNo = userDto.PhoneNo,
                IsEmailVerified = userDto.IsEmailVerified,
                UpdatedDate = userDto.UpdatedDate,
                AreaId = userDto.AreaId,
                CountryId = userDto.CountryId,
                StateId = userDto.StateId,
                CityId = userDto.CityId,
                Gender = userDto.Gender,
                IsDelete = userDto.IsDelete,
                UpdatedBy = userDto.UpdatedBy,
            };

        }

        public static UpdateUserViewModel mapping(User user)
        {
            return new UpdateUserViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Address = user.Address,
                PhoneNo = user.PhoneNo,
                Gender =  user.Gender,
                IsEmailVerified = user.IsEmailVerified,
                UpdatedDate = user.UpdatedDate,
                AreaId = user.AreaId,
                CountryId = user.CountryId,
                StateId = user.StateId,
                CityId = user.CityId,
                IsDelete = user.IsDelete,
                UpdatedBy = user.UpdatedBy,
            };

        }

        public static IEnumerable<UpdateUserViewModel> mapping(IEnumerable<User> users)
        {
            var userDtos = new List<UpdateUserViewModel>();
            foreach (var user in users)
            {
                userDtos.Add(mapping(user));
            }
            return userDtos;
        }

        public static IEnumerable<User> mapping(IEnumerable<UpdateUserViewModel> userDto)
        {
            var userList = new List<User>();
            foreach (var user in userDto)
            {
                userList.Add(mapping(user));
            }
            return userList;
        }
    }
}
