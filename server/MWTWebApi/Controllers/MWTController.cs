﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWTCore.Models.Custom;
using MWTCore.Services.Interfaces;
using MWTDbContext.Models;
using MWTWebApi.Model;
using MWTWebApi.Model.Custom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MWTWebApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableCors("AllowOrigin")]
    [Route("Account")]
    [ApiController]
    public class MWTController : ControllerBase
    {
        #region DI
        private readonly IAccountService _accountService;
        private readonly IAuthentication _authentication;

        public MWTController(IAccountService accountService, IAuthentication authentication)
        {
            _accountService = accountService;

            _authentication = authentication;
        }
        #endregion

        #region TestAPI
        [AllowAnonymous]
        [Route("TestAPI")]
        [HttpGet]
        public HttpAPIResponse TestWebAPI()
        {
            return new HttpAPIResponse()
            {
                Content = JsonConvert.SerializeObject("WebAPI is working As expected"),
                StatusCode = HttpStatusCode.OK,
                Timestamp = DateTime.Now
            };
        }
        #endregion

        #region SignUp
        [AllowAnonymous]
        [Route("SignUp")]
        [HttpPost]
        public HttpAPIResponse SignUp(User usr)

        {
            if (!_accountService.checkUsername(usr.Username).Result)
            {
                int id = _accountService.CreateUser(usr).Result;

                return new HttpAPIResponse()
                {
                    Content = JsonConvert.SerializeObject(id),
                    StatusCode = HttpStatusCode.OK,
                    Timestamp = DateTime.Now
                };
            }
            else
            {
                return new HttpAPIResponse()
                {
                    Content = JsonConvert.SerializeObject("UsernameExists"),
                    StatusCode = HttpStatusCode.OK,
                    Timestamp = DateTime.Now
                };
            }
        }
        #endregion

        #region CheckUsername
        [AllowAnonymous]
        [HttpPost("CheckUsername")]
        public HttpAPIResponse CheckUsername([FromBody] string usrname)
        {
            var response = _accountService.checkUsername(usrname).Result;

            return new HttpAPIResponse()
            {
                Content = JsonConvert.SerializeObject(response),
                StatusCode = HttpStatusCode.OK,
                Timestamp = DateTime.Now
            };
        }
        #endregion

        #region Login
        [AllowAnonymous]
        [HttpPost("Login")]
        public HttpAPIResponse Login(LoginModel usr)
        {
            var _user = _accountService.UserExists(usr.Username, usr.Password).Result;
            if (_user != null)
            {
                _user.Password = _authentication.AuthenticateData(_user.Username, _user.Role);
            }

            return new HttpAPIResponse()
            {
                Content = JsonConvert.SerializeObject(_user),
                Timestamp = DateTime.Now,
                StatusCode = HttpStatusCode.OK
            };
        }
        #endregion

        #region GetMyuser
        [Authorize(Roles = "1,2,3")]
        [HttpGet("GetMyUser/{id}")]
        public HttpAPIResponse GetMyUser(int id)
        {
            var _user = _accountService.FetchUser(id).Result;
            _user.Password = "";
            if (_user != null)
            {
                return new HttpAPIResponse()
                {
                    Timestamp = DateTime.Now,
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(_user)
                };
            }

            return new HttpAPIResponse()
            {
                Timestamp = DateTime.Now,
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(null)
            };

        }
        #endregion

        #region ChangePassword
        [Authorize(Roles = "1,2,3")]
        [HttpPost("ChangePassword")]
        public HttpAPIResponse ChangePassword(ChangePassword changePassword)
        {
            if (_accountService.CheckOldPassword(changePassword.OldPass, changePassword.id).Result)
            {

                var status = _accountService.UpdatePassword(changePassword).Result;
                return new HttpAPIResponse()
                {
                    Timestamp = DateTime.Now,
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(status)
                };
            }
            else
            {
                return new HttpAPIResponse()
                {
                    Timestamp = DateTime.Now,
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(-1)
                };
            }
        }
        #endregion

        #region UpdateUser
        [Authorize(Roles = "1,2,3,")]
        [HttpPost("UpdateUser")]
        public HttpAPIResponse UpdateUser()
        {
            var formCollection = Request.ReadFormAsync().Result;
            var file = formCollection.Files.First();
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Images", "Avatar"));
            var fileExt = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            var fileName = Guid.NewGuid().ToString("N");

            try
            {
                using (var stream = new FileStream(Path.Combine(pathToSave, fileName + fileExt), FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            catch(Exception ex)
            {
                return new HttpAPIResponse()
                {
                    Content = JsonConvert.SerializeObject(null),
                    StatusCode = HttpStatusCode.OK,
                    Timestamp = DateTime.Now
                };
            }

            var user = JsonConvert.DeserializeObject<UpdateUser>(formCollection.First().Value);

            user.Avatar = fileName + fileExt;

            var status = _accountService.UpdateUser(user).Result;

            return new HttpAPIResponse()
            {
                Content = JsonConvert.SerializeObject(status),
                StatusCode = HttpStatusCode.OK,
                Timestamp = DateTime.Now
            };
        }
        #endregion



    }
}

