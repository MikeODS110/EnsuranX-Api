using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        [HttpGet("SearchPlans")]
        public async Task<IActionResult> SearchPlans(string zipCode, int year, string market = "Individual", int age = 40, int income = 30000, bool tobacco = false, string division = "HealthCare", string metalLevel = "", bool raw = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 5 || !zipCode.All(char.IsDigit))
                    return BadRequest("Enter a valid 5-digit ZIP code.");
                if (age < 0 || age > 120)
                    return BadRequest("Enter a valid age.");
                if (market != "Individual" && market != "SHOP")
                    return BadRequest("Unsupported market.");
                if (division != "HealthCare" && division != "Dental")
                    return BadRequest("Unsupported plan division.");

                string apiKey = MarketplaceApiKey;
                if (string.IsNullOrEmpty(apiKey))
                    return StatusCode(500, "CMS API key is not configured.");

                var client = _clientFactory.CreateClient();

                var countyResp = await client.GetAsync($"https://marketplace.api.healthcare.gov/api/v1/counties/by/zip/{zipCode}?apikey={apiKey}");
                if (!countyResp.IsSuccessStatusCode)
                    return StatusCode((int)countyResp.StatusCode, "County lookup failed.");
                var countyJson = JObject.Parse(await countyResp.Content.ReadAsStringAsync());
                var counties = countyJson["counties"] as JArray;
                if (counties == null || counties.Count == 0)
                    return NotFound("No county found for that ZIP code.");
                var county = counties[0];
                string fips = (string)county["fips"];
                string state = (string)county["state"];
                string countyName = (string)county["name"];

                async Task<(bool ok, int status, string text)> SearchOnce(JObject filter, int offset)
                {
                    var person = new JObject
                    {
                        ["age"] = age,
                        ["aptc_eligible"] = true,
                        ["gender"] = "Female",
                        ["uses_tobacco"] = tobacco
                    };
                    var body = new JObject
                    {
                        ["household"] = new JObject
                        {
                            ["income"] = income,
                            ["people"] = new JArray(person),
                            ["has_married_couple"] = false
                        },
                        ["market"] = market,
                        ["place"] = new JObject
                        {
                            ["countyfips"] = fips,
                            ["state"] = state,
                            ["zipcode"] = zipCode
                        },
                        ["year"] = year,
                        ["limit"] = 10,
                        ["offset"] = offset,
                        ["order"] = "asc",
                        ["sort"] = "premium"
                    };
                    if (filter != null)
                        body["filter"] = filter;

                    var resp = await client.PostAsync(
                        $"https://marketplace.api.healthcare.gov/api/v1/plans/search?apikey={apiKey}&year={year}",
                        new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
                    var text = await resp.Content.ReadAsStringAsync();
                    return (resp.IsSuccessStatusCode, (int)resp.StatusCode, text);
                }

                var searches = new List<Task<(bool ok, int status, string text)>>();
                if (division == "Dental")
                {
                    var dentalFilter = new JObject { ["division"] = "Dental" };
                    for (int offset = 0; offset < 30; offset += 10)
                        searches.Add(SearchOnce(dentalFilter, offset));
                }
                else if (!string.IsNullOrEmpty(metalLevel))
                {
                    searches.Add(SearchOnce(new JObject { ["metal_levels"] = new JArray(metalLevel) }, 0));
                }
                else
                {
                    var tiers = new List<string> { "Bronze", "Silver", "Gold", "Platinum" };
                    if (age < 30)
                        tiers.Add("Catastrophic");
                    foreach (var tier in tiers)
                        searches.Add(SearchOnce(new JObject { ["metal_levels"] = new JArray(tier) }, 0));
                }

                var results = await Task.WhenAll(searches);
                var okResults = results.Where(r => r.ok).ToList();
                if (okResults.Count == 0)
                {
                    var first = results[0];
                    string snippet = first.text.Length > 300 ? first.text.Substring(0, 300) : first.text;
                    return StatusCode(first.status, snippet.Replace(apiKey, "***"));
                }

                var allPlans = new JArray();
                int total = 0;
                JObject firstParsed = null;
                foreach (var r in okResults)
                {
                    var parsed = JObject.Parse(r.text);
                    if (firstParsed == null)
                        firstParsed = parsed;
                    var pagePlans = parsed["plans"] as JArray;
                    if (pagePlans != null)
                    {
                        foreach (var plan in pagePlans)
                            allPlans.Add(plan);
                    }
                    if (division == "Dental")
                    {
                        if (total == 0)
                            total = (int?)parsed["total"] ?? 0;
                    }
                    else
                    {
                        total += (int?)parsed["total"] ?? 0;
                    }
                }

                if (raw)
                {
                    var rawResult = new JObject
                    {
                        ["county"] = countyName,
                        ["state"] = state,
                        ["total"] = total,
                        ["returned"] = allPlans.Count,
                        ["topLevelKeys"] = new JArray(firstParsed.Properties().Select(prop => prop.Name)),
                        ["facet_groups"] = firstParsed["facet_groups"],
                        ["sample"] = new JArray(allPlans.Take(2))
                    };
                    return Content(rawResult.ToString(Formatting.None), "application/json");
                }

                var compact = new JArray();
                foreach (var p in allPlans)
                {
                    compact.Add(new JObject
                    {
                        ["id"] = p["id"],
                        ["name"] = p["name"],
                        ["issuer"] = p["issuer"]?["name"],
                        ["issuerUrl"] = p["issuer"]?["individual_url"],
                        ["premium"] = p["premium"],
                        ["premiumWithCredit"] = p["premium_w_credit"],
                        ["metalLevel"] = p["metal_level"],
                        ["type"] = p["type"],
                        ["division"] = p["product_division"],
                        ["deductibles"] = p["deductibles"],
                        ["moops"] = p["moops"],
                        ["qualityRating"] = p["quality_rating"],
                        ["hasNationalNetwork"] = p["has_national_network"],
                        ["brochureUrl"] = p["brochure_url"],
                        ["networkUrl"] = p["network_url"],
                        ["formularyUrl"] = p["formulary_url"]
                    });
                }

                var result = new JObject
                {
                    ["county"] = countyName,
                    ["state"] = state,
                    ["fips"] = fips,
                    ["zipCode"] = zipCode,
                    ["year"] = year,
                    ["market"] = market,
                    ["division"] = division,
                    ["total"] = total,
                    ["plans"] = compact
                };
                return Content(result.ToString(Formatting.None), "application/json");
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
