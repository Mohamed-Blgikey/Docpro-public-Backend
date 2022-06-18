using AutoMapper;
using Docpro.BL.Dtos;
using Docpro.BL.Helper;
using Docpro.BL.Interface;
using Docpro.DAL.Entity;
using Docpro.DAL.extend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Docpro.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region fields
        private readonly IDynamicRep<BooKRepot> reportRep;
        private readonly IDynamicRep<Section> sectionRep;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHubContext<NotifyHub> hub;
        private readonly IDynamicRep<AvailableTimes> timeRep;

        #endregion

        #region ctor
        public UsersController(IDynamicRep<Section> sectionRep,IDynamicRep<BooKRepot> ReportRep, UserManager<ApplicationUser> userManager,IMapper mapper, RoleManager<IdentityRole> roleManager,IHubContext<NotifyHub> hub,IDynamicRep<AvailableTimes> timeRep)
        {
            this.sectionRep = sectionRep;
            this.userManager = userManager;
            this.mapper = mapper;
            this.roleManager = roleManager;
            this.hub = hub;
            this.timeRep = timeRep;
            reportRep = ReportRep;

        }
        #endregion

        #region Get all sections
        [HttpGet]
        [Route("~/GetSections")]
        public async Task<IActionResult> GetSections()
        {
            try
            {
                var Section = await sectionRep.GetAllAsync(s => true, new[] { "Doctors" });
                if (Section == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Message = "success",
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Error = "There were no users yet"
                    });
                }

                var sections = mapper.Map<IEnumerable<SectionForReturnDto>>(Section);
                return Ok(new ApiResponse<IEnumerable<SectionForReturnDto>>
                {
                    Message = "success",
                    Code = "200",
                    Status = "ok",
                    Count = sections.Count(a => true),
                    Data = sections
                });
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        #region GetAvailableTimes
        [HttpGet]
        [Route("~/GetAvailableTimes/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailabletime(string id)
        {
            try
            {
                var times = await timeRep.GetAllAsync(a => a.doctorId == id, null);
                return Ok(new ApiResponse<IEnumerable<AvailableTimes>>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = times.Count(a => true),
                    Data = times.OrderBy(a => a.Day).ThenByDescending(a => a.From).ThenByDescending(a => a.To).ToList()
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region GetPatients
        [HttpGet]
        [Authorize(Roles ="Doctor,Admin")]
        [Route("~/GetPatients")]
        public async Task<IActionResult> GetPatients()
        {
            try
            {
                var users = await userManager.Users.Where(u => u.Status == "Patient" ).ToListAsync();
                if (users == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Message = "success",
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Error = "There were no users yet"
                    });
                }

                var userRetuen = mapper.Map<IEnumerable<UserForReturnDto>>(users);
                return Ok(new ApiResponse<IEnumerable<UserForReturnDto>>
                {
                    Message = "success",
                    Code = "200",
                    Status = "ok",
                    Count = users.Count,
                    Data = userRetuen
                });
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        

        #region Get User
        [HttpGet]
        [Route("~/GetUser/{id}")]

        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Message = "success",
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Error = $"There were no user with this id : {id}"
                    });
                }
                var userReturn = mapper.Map<UserForReturnDto>(user);
                return Ok(new ApiResponse<UserForReturnDto>
                {
                    Message = "success",
                    Code = "200",
                    Status = "ok",
                    Count = 1,
                    Data = userReturn
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Edit User
        [HttpPut]
        [Route("~/EditUser")]
        public async Task<IActionResult> EditUser(UserForUpdateDto userForUpdateDto)
        {
            if (userForUpdateDto.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                return Unauthorized();
            try
            {
                var error = string.Empty;
                if (ModelState.IsValid)
                {

                    var user = await userManager.FindByIdAsync(userForUpdateDto.Id);
                    var emailExsit = await userManager.FindByEmailAsync(userForUpdateDto.Email);
                    if (emailExsit != null && userForUpdateDto.Email != user.Email)
                    {
                        return Ok(new ApiResponse<string>
                        {
                            Code = "200",
                            Status = "Ok",
                            Message = "Done!",
                            Count = 0,
                            Error = "Email Is Already Token"
                        });
                    }


                    user.UserName = userForUpdateDto.Email;
                    user.Email = userForUpdateDto.Email;
                    user.FirstName = userForUpdateDto.FirstName;
                    user.LastName = userForUpdateDto.LastName;

                    var result = await userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        foreach (var item in result.Errors)
                        {
                            error += $"{item.Description},";
                        }
                        return NotFound(new ApiResponse<string>
                        {
                            Code = "404",
                            Status = "Not Found",
                            Count = 0,
                            Message = "Error",
                            Error = error
                        });
                    }
                    var userReturn = mapper.Map<UserForReturnDto>(user);
                    await hub.Clients.All.SendAsync("EditUser");
                    return Ok(new ApiResponse<UserForReturnDto>
                    {
                        Code = "200",
                        Status = "Ok",
                        Count = 1,
                        Message = "Data Updating",
                        Data = userReturn
                    });
                }
                foreach (var item in ModelState)
                {
                    error += $"{item.Value.Errors}";
                }
                return NotFound(new ApiResponse<string>
                {
                    Code = "404",
                    Status = "Not Found",
                    Message = "Error",
                    Error = error
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #region SavePhoto

        [HttpPost]
        [Route("~/SavePhoto/{id}")]
        public async Task<IActionResult> SavePhoto(string id)
        {
            if (id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                return Unauthorized();
            try
            {
                var error = string.Empty;
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = Guid.NewGuid() + postedFile.FileName;
                var physicalPath = Directory.GetCurrentDirectory() + "/wwwroot/Img/" + filename;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await postedFile.CopyToAsync(stream);
                }

                var user = await userManager.FindByIdAsync(id);
                user.PhotoName = filename;
                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var item in result.Errors)
                    {
                        error += $"{item.Description},";
                    }
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Message = "Error",
                        Error = error
                    });
                }
                var userReturn = mapper.Map<UserForReturnDto>(user);
                await hub.Clients.All.SendAsync("EditUser");
                return Ok(new ApiResponse<UserForReturnDto>
                {
                    Code = "200",
                    Status = "Ok",
                    Count = 1,
                    Message = "Data Updating",
                    Data = userReturn
                });

            }
            catch (Exception)
            {
                return Ok(new { message = "Error !" });
            }
        }
        #endregion

        #region UnsavePhoto
        [HttpPost]
        [Route("~/UnSavePhoto")]
        public async Task<IActionResult> UnSaveFile([FromBody] PhotoDto photoVM)
        {
            if (photoVM.UserId != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                return Unauthorized();
            try
            {
                var error = string.Empty;

                if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "/wwwroot/Img/" + photoVM.Name) && photoVM.Name != "defualt.png")
                {
                    System.IO.File.Delete(Directory.GetCurrentDirectory() + "/wwwroot/Img/" + photoVM.Name);
                   
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Count = 1,
                        Message = "Data Updating",
                        Data = photoVM.Name,
                    });
                }
                return new JsonResult(new { message = "Photo NotFound !" });
            }
            catch (Exception)
            {

                return new JsonResult("Error!");
            }
        }
        #endregion

        #region UplaodPhoto
        [Route("~/UplaodPhoto")]
        [HttpPost]
        public async Task<IActionResult> UplaodPhoto()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = Guid.NewGuid() + postedFile.FileName;
                var physicalPath = Directory.GetCurrentDirectory() + "/wwwroot/Img/" + filename;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await postedFile.CopyToAsync(stream);
                }

                return Ok(new { message = filename });
            }
            catch (Exception)
            {
                return Ok(new { message = "Error !" });
            }
        }
        #endregion

        #region GetReports
        [HttpGet("~/GetReports")]
        public async Task<IActionResult> GetReports([FromQuery] PagedParams pagedParams)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await userManager.FindByNameAsync(userId);
            var Data = await reportRep.GetPageniation(pagedParams,r=>r.DoctorId == userId || r.PatientId == userId, new[] {"Patient","Doctor"} );
            var DataForReturn = mapper.Map<IEnumerable<ReportForReturnDto>>(Data);
            return Ok(new
            {
                CurrentPage = Data.CurrentPage,
                PageSize = Data.PageSize,
                TotalItems = Data.TotalCount,
                TotalPage = Data.TotalPage,
                Data = DataForReturn
            }) ;
        }
        #endregion
    }

}
