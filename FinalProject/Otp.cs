using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using FinalProject.Dtos;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using FinalProject.DBContext;
using FinalProject.Controllers;

namespace FinalProject
{
    class Otp
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> LoginWithOtpStep1([Required] string phoneNumber, [Required] int UserId)
        {
            if (!_securityService.PhoneValidator(phoneNumber))
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "شماره همراه وارد شده نامعتبر است"
                });
            }

            var verifyKey = GenerateHashedKey(phoneNumber);
            string nationalCode = string.Empty;
            

            if (MemoryCache.TryGetValue(verifyKey, out CacheOtpDto otpDataFromCache))
            {
                if (otpDataFromCache.ExpirationTime >= DateTime.Now)
                {
                    var differenceInSecond = (int)(otpDataFromCache.ExpirationTime - DateTime.Now).TotalSeconds;
                    return BadRequest(new ApiResponse
                    {
                        Status = false,
                        StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                        Message = $"برای ارسال دوباره کد لطفا {differenceInSecond} ثانیه صبر کنید"
                    });
                }
            }

            var profileResult = await _taxishopApiService.ProfileService(phoneNumber);
            if (profileResult.ErrCode == 74)
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "شماره موبایل وارد شده نامعتبر است"
                });
            }
            if (profileResult.Result is List<ProfileServiceResult> resultList)
            {
                if (resultList.Count == 0 || resultList[0].CUST_TYPE == "حقوقی")
                    return BadRequest(new ApiResponse
                    {
                        Status = false,
                        StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                        Message = "شماره موبایل وارد شده نامعتبر است"
                    });
                else
                    nationalCode = resultList[0].CODEMELLI;
            }

            var validList = GetSimTypeValidList(UserId);
            var getSimTypeResult = await _taxishopApiService.GetSimTypeService(phoneNumber);
            if (validList.Count != 0 && !validList.Contains(int.Parse(getSimTypeResult.Result.IdType)))
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "شماره موبایل وارد شده نامعتبر است"
                });
            }

            var userExist = await TaxiShopAppDbContext.Users.AnyAsync(u => u.NationalCode == nationalCode);
            if (!userExist)
            {
                var newUser = new User
                {
                    NationalCode = nationalCode,
                    PhoneNumber = phoneNumber,
                    CreatedAt = DateTime.Now
                };
                await TaxiShopAppDbContext.Users.AllAsync(newUser);
                await TaxiShopAppDbContext.SaveChangesAsync();
            }


            // Otp Section
            Random random = new Random();
            int Number = random.Next(1000, 10000);
            var message = $"به اپلیکیشن تاکسی شاپ خوش آمدید.\n کد یکبار مصرف: \n{Number}";
            if (UserId == 4)
            {
                message = $"به اپلیکیشن تاکسی شاپ خوش آمدید.\n کد یکبار مصرف: \n{Number}";
            }
            var sendSMSResult = await _taxishopApiService.SendSMSService(message, phoneNumber);
            if (sendSMSResult.ErrCode != 0)
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = sendSMSResult.Message
                });
            }

            var cacheOtp = new CacheOtpDto
            {
                OtpCode = otpCode,
                NatoinalCode = nationalCode,
                ExpirationTime = DateTime.Now.AddMinutes(2),
            };

            MemoryCache.Set(verifyKey, cacheOtp, TimeSpan.FromMinutes(3));

            return Ok(new ApiResponse<string>
            {
                Status = true,
                StatusCode = (int)ApiStatusCodeEnum.Successful,
                Data = verifyKey
            });
        }

        private static IActionResult BadRequest(ApiResponse apiResponse)
        {
            var json = JsonConvert.SerializeObject(apiResponse);
            var contentResult = new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = (int)System.Net.HttpStatusCode.BadRequest
            };

            return contentResult;

        }

        static string GenerateHashedKey(string phonenumber)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(phonenumber);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LoginWithOtpStep2([FromBody] VerifyOtpDto verifyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "تمامی فیلد ها الزامی اند"
                });
            }

            if (!MemoryCache.TryGetValue(verifyDto.VerifyKey, out CacheOtpDto otpDataFromCache))
            {
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "کد ارسال شده منقضی شده است، لطفا کد جدید دریافت کنید"
                });
            }

            if (otpDataFromCache.FailedCount >= 3)
            {
                MemoryCache.Remove(verifyDto.VerifyKey);
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = $"{otpDataFromCache.FailedCount} بار کد را اشتباه وارد کردید لطفا کد جدید دریافت کنید"
                });
            }

            if (otpDataFromCache.ExpirationTime <= DateTime.Now)
            {
                MemoryCache.Remove(verifyDto.VerifyKey);
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "کد ارسال شده منقضی شده است، لطفا کد جدید دریافت کنید"
                });
            }

            if (otpDataFromCache.OtpCode != verifyDto.OtpCode)
            {
                otpDataFromCache.FailedCount++;
                return BadRequest(new ApiResponse
                {
                    Status = false,
                    StatusCode = (int)ApiStatusCodeEnum.BadRequest,
                    Message = "کد وارد شده نادرست است"
                });
            }

            var user = await TaxiShopAppDbContext.Users.FirstOrDefaultAsync(u => u.NationalCode == otpDataFromCache.NatoinalCode);
            var jwtTokenData = await TokenFactoryService.CreateJwtTokensAsync(user);

            MemoryCache.Remove(verifyDto.VerifyKey);

            return Ok(new ApiResponse<JwtTokensData>
            {
                Status = true,
                StatusCode = (int)ApiStatusCodeEnum.Successful,
                Message = "باموفقیت وارد حساب کاربری خود شدید",
                Data = jwtTokenData
            });
        }
    }
}
