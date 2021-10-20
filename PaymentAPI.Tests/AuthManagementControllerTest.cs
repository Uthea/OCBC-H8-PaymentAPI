using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PaymentAPI.Controllers;
using PaymentAPI.Models.DTOs.Requests;
using PaymentAPI.Models.DTOs.Responses;
using PaymentAPI.Services;
using Xunit;

namespace PaymentAPI.Tests
{
    public class AuthManagementControllerTest : ControllerBase
    {
        
        
        [Fact]
        public void Should_ReturnJSONResult_WhenRegisterSucceed()
        {
            var _authManagementService = new Mock<IAuthManagementService>();
            AuthManagementController _authManagementController = new AuthManagementController(_authManagementService.Object);
            _authManagementService.Setup(
                s => s.FindUserByEmail(It.IsAny<String>())).ReturnsAsync((IdentityUser)null);
             _authManagementService.Setup(
                 s => s.CreateNewUser(It.IsAny<UserRegistrationDto>())).ReturnsAsync(IdentityResult.Success);

             var userDTO = new UserRegistrationDto();
             userDTO.Email = "test@gmail.com";
             userDTO.Password = "test";
             userDTO.Username = "halo123";

             var result = _authManagementController.Register(userDTO);
             
             //Assert.Equal(TaskStatus.RanToCompletion, result.Status);
             Assert.IsType<JsonResult>(result.Result);

        }
        
         [Fact]
         public void Should_ReturnBadRequestObjectResult_WhenEmailExistAtRegister()
         {
           var userDTO = new UserRegistrationDto
           {
               Email = "test@gmail.com",
               Password = "test",
               Username = "halo123"
           };

           var userIdentity = new IdentityUser
           {
               Email = "test@gmail.com"
           };
             
             var _authManagementService = new Mock<IAuthManagementService>();
             AuthManagementController _authManagementController = new AuthManagementController(_authManagementService.Object);
             _authManagementService.Setup(
                 s => s.FindUserByEmail(It.IsAny<String>())).ReturnsAsync(userIdentity);
             
             var result = _authManagementController.Register(userDTO);

             Assert.IsType<BadRequestObjectResult>(result.Result);

         }       
        
        
    }
}