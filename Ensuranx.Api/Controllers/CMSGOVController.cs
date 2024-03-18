using Azure;
using ErrorOr;
using Ensuranx.Api.Middlewares;
using System.Net.Http;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.UserInfo;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Role;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Services.Identity;
using Ensuranx.Infrastructure.Services.UserInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Azure;
using ErrorOr;
using Ensuranx.Api.Middlewares;
using System.Net.Http;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.UserInfo;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Role;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Services.Identity;
using Ensuranx.Infrastructure.Services.UserInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection.Emit;
using static System.Net.WebRequestMethods;
using Ensuranx.Api.Controllers;
using System.Runtime.CompilerServices;
using Ensuranx.Application.Contracts.Providers;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Api.Controllers
{
    [Route("api/[controller]")]
    public class CMSGOVController : ApiBaseController
    {

        private readonly IHttpClientFactory _clientFactory;
        public CMSGOVController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
          
        }

        [HttpGet("PostHealthInsuranceInfo")]

        public async Task<IActionResult> PostHealthInsuranceInfo(string zipCode)
        {
            try
            {
                string apiKey = "d687412e7b53146b2631dc01974ad0a4";
                //string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/counties/by/zip/{zipCode}?apikey=${apikey}";

                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/counties/by/zip/27360?apikey=d687412e7b53146b2631dc01974ad0a4";
                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("apikey", apiKey);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var responseData = JsonConvert.DeserializeObject(responseBody);

                    
                    return Ok(responseBody);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve data.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

    }
}
