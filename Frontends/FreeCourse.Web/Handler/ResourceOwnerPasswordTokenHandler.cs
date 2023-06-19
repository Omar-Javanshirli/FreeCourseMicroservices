using FreeCourse.Web.Exceptions;
using FreeCourse.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FreeCourse.Web.Handler
{
    public class ResourceOwnerPasswordTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;
        private readonly ILogger<ResourceOwnerPasswordTokenHandler> _logger;

        public ResourceOwnerPasswordTokenHandler(IHttpContextAccessor httpContextAccessor, IIdentityService identityService, ILogger<ResourceOwnerPasswordTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //ilk once access tokeni elde edirik
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync
                (OpenIdConnectParameterNames.AccessToken);

            //sora elde etdiyimiz access tokeni requestin headerina elave edirik
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            //daha sonra gondereceyimiz requestin response izleyirik bize ne qaytracag deye.
            var response = await base.SendAsync(request, cancellationToken);

            ////yoxlanisi edirik egerki access token kecersizdirse. refresh token elde edeceyik.
            if (response is { StatusCode: System.Net.HttpStatusCode.Unauthorized })
            {
                //Refresh token elde edirik access token kecersiz oldgu ucun 
                var tokenResponse = await _identityService.GetAccessTokenByRefreshToken();

                //gelen datani yoxalyiriq
                if (tokenResponse != null)
                {
                    //refresh token vasitesi ile yeni elde etdiyimiz tokeni requestin headerina elave edirik
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue
                        ("Bearer", tokenResponse.AccessToken);

                    //yeiden request gonderirik ve respnse izleyirik
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            //eger yene bize elde etdiytimiz yeni access tokennende Unauthorize gelibse demek token kecersizdir
            //ve logglama edirik
            if (response is { StatusCode: System.Net.HttpStatusCode.Unauthorized })
                throw new UnAuthorizeException();

            return response;
        }
    }
}