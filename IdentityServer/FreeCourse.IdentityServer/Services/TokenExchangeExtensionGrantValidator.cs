using IdentityServer4.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.IdentityServer.Services
{
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        //istedeyiniz adalandirmani vere bilersiz
        public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
        private readonly ITokenValidator _tokenValidator;

        public TokenExchangeExtensionGrantValidator(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            //requesti elde edirik.
            var requestRaw = context.Request.Raw.ToString();

            //requestin tokenini elde edirik
            var token = context.Request.Raw.Get("subject_token");

            //tokenin yoxlanmagi
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new GrantValidationResult
                    (IdentityServer4.Models.TokenRequestErrors.InvalidRequest, "token missing");
                return;
            }

            //elde etdiyimiz tokeni dogruluyurux. Imzasi,vaxtinin bitim bitmediyini ve s.
            var tokenValidateResult = await _tokenValidator.ValidateAccessTokenAsync(token);

            if (tokenValidateResult.IsError)
            {
                context.Result = new GrantValidationResult
                    (IdentityServer4.Models.TokenRequestErrors.InvalidGrant, "token invalid");
                return;
            }

            //tokenin paylod hissesinde itifadecinin id-sini saxladigi "sub" claimini elde edirik.
            var subjectClaim = tokenValidateResult.Claims.FirstOrDefault(x => x.Type == "sub");

            if (subjectClaim == null)
            {
                context.Result = new GrantValidationResult
                    (IdentityServer4.Models.TokenRequestErrors.InvalidRequest, "token must contain sub value");
                return;
            }

            //istifadecinin id-sini, access token oldugunu bildirirk ve tokeninin icinde ki claimlari gonderirik. 
            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token",
               tokenValidateResult.Claims);

            return;
        }
    }
}