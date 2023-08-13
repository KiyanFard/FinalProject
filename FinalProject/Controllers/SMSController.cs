using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FinalProject.Controllers;
using FinalProject.Dtos;
using FinalProject;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Controllers
{
    public class SMSController
    {
        public class Api
        {
            const string BaseUrl = "https://smsdemo.pajal.net";
            const string EndPointSMS = "/api/v1/GeneralSms/SendSms";
            const string EndPointLogin = "/api/user/Login";


            public static async Task SendSMS()
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/api/v1/GeneralSms/SendSms"))
                    {

                        JwtDto dto = new JwtDto();
                        var Login = await LoginAsync();
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Login);

                        //Generate a Random Four-Digit Number
                        Random random = new Random();
                        int Number = random.Next(1000, 10000);

                        //Properties phonenumber = new Properties();

                        var requestBody = JsonConvert.SerializeObject(new
                        {
                            SendSmsInfo = new
                            {
                                From = "989999175598",
                                Message = $"{Number}",
                                To = $"{dto.PhoneNumber}"
                            }
                        });

                        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                        request.Content = content;

                        //Send a POST request and get the response
                        var apiResponse = await httpClient.SendAsync(request);

                        Console.WriteLine(apiResponse);

                        //Read the response content as a string
                        var ResponseContent = await apiResponse.Content.ReadAsStringAsync();

                        Console.WriteLine(ResponseContent);
                    }
                }
            }

            public static async Task<string> LoginAsync()
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/api/user/Login"))
                    {
                        request.Content = new StringContent("grant_type=password&username=User.net&password=123", Encoding.UTF8, "application/x-www-form-urlencoded");

                        var response = await httpClient.SendAsync(request);

                        // read the response body as a string
                        var responseBody = await response.Content.ReadAsStringAsync();

                        var jwtdto = JsonConvert.DeserializeObject<JwtDto>(responseBody);

                        Console.WriteLine(jwtdto.access_token);

                        return jwtdto.access_token;
                    }
                }
            }
        }
    }
}

