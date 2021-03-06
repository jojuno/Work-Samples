﻿using SocialMediaApp132.Models.Domain.App;
using SocialMediaApp132.Models.Domain.Tools;
using SocialMediaApp132.Models.Requests.App;
using SocialMediaApp132.Models.Requests.ForgotPassword;
using SocialMediaApp132.Models.Responses;
using SocialMediaApp132.Services;
using SocialMediaApp132.Services.App;
using SocialMediaApp132.Services.Tools;
using System.Threading.Tasks;
using SocialMediaApp132.Models.Requests.ChangePassword;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace SocialMediaApp132.Web.Controllers.Api.App
{
    [AllowAnonymous]
    [RoutePrefix("api/App/AppTokens")]
    public class AppTokenController : ApiController
    {
        IAppTokenService _appTokenService;
        IEmailMessenger _emailMessenger;
        IAppLogService _appLogService;
        IUserService _userService;
        IEmailTemplateService _emailTemplateService;
        private int currentUserId;

        public AppTokenController(IAppTokenService appTokenService, IEmailMessenger emailMessenger, IAppLogService appLogService, IUserService userService, IEmailTemplateService emailTemplateService)
        {
            _appTokenService = appTokenService;
            _emailMessenger = emailMessenger;
            _appLogService = appLogService;
            _userService = userService;
            _emailTemplateService = emailTemplateService;
            currentUserId = _userService.GetCurrentUserId();
        }

        [Route(), HttpGet]
        public IHttpActionResult GetAll()
        {
            try
            {
                ItemsResponse<AppToken> response = new ItemsResponse<AppToken>();
                response.Items = _appTokenService.ReadAll();
                return Ok(response);
            }
            catch (Exception ex)
            {
                int currentUser = _userService.GetCurrentUserId();
                _appLogService.Insert(new AppLogAddRequest
                {
                    AppLogTypeId = 1,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Title = "Error in " + GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    UserBaseId = currentUser
                });

                return BadRequest(ex.Message);
            }
        }

        [Route(), HttpPost]
        public IHttpActionResult Post(AppTokenAddRequest model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _appTokenService.Insert(model);
                return Ok(new SuccessResponse());
            }
            catch (Exception ex)
            {
                int currentUser = _userService.GetCurrentUserId();
                _appLogService.Insert(new AppLogAddRequest
                {
                    AppLogTypeId = 1,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Title = "Error in " + GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    UserBaseId = currentUser
                });

                return BadRequest(ex.Message);
            }
        }


        [AllowAnonymous]
        [Route("forgotpassword"), HttpPost]
        public IHttpActionResult InsertGUID(ForgotPasswordAppTokenAddRequest model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                ItemResponse<string> response = new ItemResponse<string>
                {
                    Item = _appTokenService.InsertGUID(model)
                };

                if (response.Item != null)
                {
                    Email eml = new Email();

                    MessageAddress msgAdd = new MessageAddress
                    {
                        Email = model.Email,
                        Name = model.Email //change later when I can grab name of user
                    };

                    List<MessageAddress> list = new List<MessageAddress>
                    {
                        msgAdd
                    };

                    eml.To = list;
                    eml.FromAddress = "Eleveightc56@gmail.com";
                    eml.FromName = "Eleveight";
                    eml.Subject = "Reset your password";
                    eml.HtmlBody = _emailTemplateService.CreateForgotPassword(new EmailTemplateInput
                    {
                        Name = msgAdd.Name,
                        Token = response.Item
                    });

                    _emailMessenger.SendMail(eml);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                int currentUser = _userService.GetCurrentUserId();
                _appLogService.Insert(new AppLogAddRequest
                {
                    AppLogTypeId = 1,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Title = "Error in " + GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    UserBaseId = currentUser
                });

                return BadRequest(ex.Message);
            }
        }


        [AllowAnonymous]
        [Route("{id}"), HttpPut]
        public IHttpActionResult UpdatePassword(ForgotPasswordUserBaseUpdateRequest model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _appTokenService.UpdatePassword(model);
                return Ok(new SuccessResponse());
            }
            catch (Exception ex)
            {
                int currentUser = _userService.GetCurrentUserId();
                _appLogService.Insert(new AppLogAddRequest
                {
                    AppLogTypeId = 1,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Title = "Error in " + GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    UserBaseId = currentUser
                });

                return BadRequest(ex.Message);
            }
        }

        [Route("changePassword"), HttpPut]
        public IHttpActionResult ChangePassword(ChangePasswordUserBaseUpdateRequest model)
        {
            try
            {
                model.CurrentUserBaseId = currentUserId;
                if (!ModelState.IsValid) return BadRequest(ModelState);
                Boolean isPasswordChanged = _appTokenService.ChangePassword(model);
                if (!isPasswordChanged) return BadRequest(ModelState);
                //if the passwords fail, don't return OK
                return Ok(new SuccessResponse());
            }
            catch (Exception ex)
            {
                int currentUser = _userService.GetCurrentUserId();
                _appLogService.Insert(new AppLogAddRequest
                {
                    AppLogTypeId = 1,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Title = "Error in " + GetType().Name + " " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    UserBaseId = currentUser
                });

                return BadRequest(ex.Message);
            }
        }
    }
}
