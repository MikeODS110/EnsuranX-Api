using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Ensuranx.Application.Requests.CMS.GOV;
using System.Net.Http.Headers;

namespace Ensuranx.Api.Controllers
{
    [Route("api/[controller]")]
    public class CMSGOVController : ApiBaseController
    {

        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        public CMSGOVController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        private string CountyApiKey => _configuration["CMSGOV:CountyApiKey"];
        private string MarketplaceApiKey => _configuration["CMSGOV:MarketplaceApiKey"];

        [HttpGet("PostHealthInsuranceInfo")]
       // [Authorize(Roles = "User")]
        public async Task<IActionResult> PostHealthInsuranceInfo(string zipCode)
        {
            try
            {
                string apiKey = CountyApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/counties/by/zip/{zipCode}?apikey={apiKey}";
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

        [HttpGet("AutoComplete")]
        public async Task<IActionResult> Autocomplete(string zipCode, string q, string type)
        {
            try
            {
                string apiKey = CountyApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/providers/autocomplete?apikey={apiKey}&q={q}&zipcode={zipCode}&type={type}";
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

        [HttpGet("PlansById")]
        public async Task<IActionResult> GetPlansByPlanId(string planId, string year)
        {
            try
            {
                string apiKey = MarketplaceApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/plans/{planId}?year={year}&apikey={apiKey}";
                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

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
        [HttpGet("IssuerList")]
        public async Task<IActionResult> GetIssuerList()
        {
            try
            {
                string apiKey = MarketplaceApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/issuers?apikey={apiKey}";

                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

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
        [HttpGet("IssuerDetail")]
        public async Task<IActionResult> GetIssuerDetail(long issuerId)
        {
            try
            {
                string apiKey = MarketplaceApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/issuers/{issuerId}?apikey={apiKey}";

                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

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
        [HttpGet("DrugSearch")]
        public async Task<IActionResult> GetDrugSearch(string drugSearch)
        {
            try
            {
                string apiKey = MarketplaceApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/drugs/search?apikey={apiKey}&q={drugSearch}";

                var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

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

        [HttpPost("GetMultiplePlans")]
        public async Task<IActionResult> GetMultiplePlans(string year, [FromBody] PlanRequest requestBody)
        {
            try
            {
                string apiKey = CountyApiKey;
                string apiUrl = $"https://marketplace.api.healthcare.gov/api/v1/plans?apikey={apiKey}&year={year}";
                string jsonBody = JsonConvert.SerializeObject(requestBody);

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(responseContent);
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
