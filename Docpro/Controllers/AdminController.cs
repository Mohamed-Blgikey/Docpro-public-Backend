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
using System.Linq;

namespace Docpro.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        #region fields
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;
        private readonly IHubContext<NotifyHub> hub;
        private readonly IDynamicRep<Section> sectionRep;
        private readonly IDynamicRep<Request> rep;
        #endregion

        #region Ctor
        public AdminController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager, IMapper mapper, IHubContext<NotifyHub> hub,IDynamicRep<Section> sectionRep, IDynamicRep<Request> rep)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
            this.hub = hub;
            this.sectionRep = sectionRep;
            this.rep = rep;
        } 
        #endregion

        #region Get All Users
        [HttpGet]
        [Route("~/GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await userManager.Users.Where(u => u.Status != "Admin").ToListAsync();
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

        #region Delete User For Admin Only
        [HttpPost]
        [Route("~/DeleteUser/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                var error = string.Empty;
                if (user == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "User Not Exsit"
                    });
                }

                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "Operation Done !",
                        Error = "Admin Can't Delete himself"
                    });
                }

                var result = await userManager.DeleteAsync(user);

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
                        Message = "Error",
                        Error = error
                    });
                }
                else
                {
                    if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "/wwwroot/Img/" + user.PhotoName) && user.PhotoName != "defualt.png")
                    {
                        System.IO.File.Delete(Directory.GetCurrentDirectory() + "/wwwroot/Img/" + user.PhotoName);
                    }
                    if (user.Status == "Doctor")
                    {
                        await hub.Clients.All.SendAsync("DeleteDoctor");
                    }
                    else
                    {
                        await hub.Clients.All.SendAsync("DeletePatient");
                    }
                    return Ok(new ApiResponse<ApplicationUser>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "User Deleted !",
                        Data = user
                    });
                }



            }
            catch (Exception ex)
            {
                throw;
            }
            return Ok();
        }
        #endregion

        #region Remove user form Role for admin only
        [HttpPost]
        [Route("~/RemoveFromRole")]
        public async Task<IActionResult> RemoveFromRole([FromBody] MangeRolesDto mangeRolesVM)
        {
            try
            {
                var role = await roleManager.FindByNameAsync(mangeRolesVM.RoleName);
                var user = await userManager.FindByNameAsync(mangeRolesVM.UserName);
                if (role == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "Role Not Exsit"
                    });
                }

                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "User Not Exsit"
                    });
                }

                if (!(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "OK",
                        Message = "Operation Done!",
                        Error = "User already out of role"
                    });
                }

                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "Operation Done !",
                        Error = "Admin Can't Delete himself"
                    });
                }

                IdentityResult result = null;

                if ((await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }

                if (!result.Succeeded)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "XXXXXXXXXXXXXXXXXXXXXX"
                    });
                }


                user.Status = "None";
                await userManager.UpdateAsync(user);
                await hub.Clients.All.SendAsync("EditUser");
                return Ok(new ApiResponse<MangeRolesDto>
                {
                    Code = "200",
                    Status = "OK",
                    Message = "Operation Done!",
                    Data = mangeRolesVM
                });

            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion

        #region Add user form Role for admin only
        [HttpPost]
        [Route("~/AddInRole")]
        public async Task<IActionResult> AddInRole([FromBody] MangeRolesDto mangeRolesVM)
        {
            try
            {
                var role = await roleManager.FindByNameAsync(mangeRolesVM.RoleName);
                var user = await userManager.FindByNameAsync(mangeRolesVM.UserName);
                if (role == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "Role Not Exsit"
                    });
                }

                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "User Not Exsit"
                    });
                }

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "OK",
                        Message = "Operation Done!",
                        Error = "User already in role"
                    });
                }



                IdentityResult result = null;

                if (!await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }

                if (!result.Succeeded)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "Error",
                        Error = "XXXXXXXXXXXXXXXXXXXXXX"
                    });
                }
                await userManager.RemoveFromRoleAsync(user, user.Status);
                user.Status = role.Name;
                if (role.Name == "Patient")
                {
                    user.Degree = null;
                    user.SectionId = null;
                    await hub.Clients.All.SendAsync("EditUserRoleToPatient");
                }
                await userManager.UpdateAsync(user);
                var userReturn = mapper.Map<UserForReturnDto>(user);
                await hub.Clients.All.SendAsync("EditUserRole");
                return Ok(new ApiResponse<UserForReturnDto>
                {
                    Code = "200",
                    Status = "OK",
                    Message = "Operation Done!",
                    Data = userReturn
                });

            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion

        #region Get Roles For Admin Only
        [HttpGet]
        [Route("~/GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var Roles = await roleManager.Roles.Where(r => r.Name != "Admin").ToListAsync();
                if (Roles == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Message = "success",
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Error = "There were no Roles yet"
                    });
                }


                return Ok(new ApiResponse<IEnumerable<IdentityRole>>
                {
                    Message = "success",
                    Code = "200",
                    Status = "ok",
                    Count = Roles.Count,
                    Data = Roles
                });
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion


        

        #region Get section
        [HttpGet]
        [Route("~/GetSection/{id}")]
        public async Task<IActionResult> GetSection(int id)
        {
            try
            {
                var Section = await sectionRep.GetByIdAsync(s=>s.Id == id, new[] { "Doctors" });
                if (Section == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Message = "success",
                        Code = "404",
                        Status = "Not Found",
                        Count = 0,
                        Error = "There were no Section match"
                    });
                }

                var sections = mapper.Map<SectionForReturnDto>(Section);
                return Ok(new ApiResponse<SectionForReturnDto>
                {
                    Message = "success",
                    Code = "200",
                    Status = "ok",
                    Count = 1,
                    Data = sections
                });
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion


        #region Add section
        [HttpPost]
        [Route("~/AddSection")]
        public async Task<IActionResult> AddSection(SectionForCudDto dto)
        {
            try
            {
                var section = mapper.Map<Section>(dto);
                var result =  sectionRep.Add(section);
                if (result != null)
                {
                    hub.Clients.All.SendAsync("AddSection");
                    return Ok(new ApiResponse<Section>
                    {
                        Message = "success",
                        Code= "200",
                        Status = "Ok",
                        Count=1,
                        Data = result,
                    });
                }
                return Ok(new ApiResponse<string>
                {
                    Message="not success",
                    Code="404",
                    Status="Not Found",
                    Count= 0,
                    Error = "error",
                });
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        #region Edit section
        [HttpPut]
        [Route("~/EditSection")]
        public async Task<IActionResult> EditSection(SectionForCudDto dto)
        {
            try
            {
                var find = sectionRep.GetById(dto.Id);
                if (find == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "not success",
                        Count = 1,
                        Error = "Section not Found"
                    });
                }

                find.Name = dto.Name;
                find.PhotoName = dto.PhotoName;
                sectionRep.SaveAllAsync();
                hub.Clients.All.SendAsync("AddSection", new {Id = 2});
                return Ok(new ApiResponse<Section>
                {
                    Message = "success",
                    Code = "200",
                    Status = "Ok",
                    Count = 1,
                    Data = find,
                });
            }
            catch (Exception)
            {
                return Ok(new ApiResponse<string>
                {
                    Message = "not success",
                    Code = "404",
                    Status = "Not Found",
                    Count = 0,
                    Error = "error",
                });

            }

        }
        #endregion

        #region Add Doctor in section
        [HttpPost]
        [Route("~/addDoctorToSection")]
        public async Task<IActionResult> addDoctorToSection(AddDoctorToSectionDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Message= "not success",
                    Code= "404",
                    Status= "Not founded",
                    Error = "Doctor not founded",
                    Count = 0,
                });
            }
            if (sectionRep.GetById(dto.sectionId) == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Message = "not success",
                    Code = "404",
                    Status = "Not founded",
                    Error = "Section not founded",
                    Count = 0,
                });
            }
            user.SectionId = dto.sectionId;
            sectionRep.SaveAllAsync();
            await hub.Clients.All.SendAsync("addDoctorToSection");
            return Ok(new ApiResponse<string>
            {
                Message = "success",
                Code = "200",
                Status = "Ok",
                Data = "Doctor Added",
                Count = 1,
            });
        }
        #endregion


        #region Remove all doctor from section
        [HttpPost]
        [Route("~/RemoveAllDoctorsFromSection/{id}")]
        public async Task<IActionResult> RemoveAllDoctors(int id)
        {
            try
            {
                var users = await userManager.Users.Where(u => u.SectionId == id).ToListAsync();

                foreach (var user in users)
                {
                    user.SectionId = null;
                }

                sectionRep.SaveAllAsync();
            return Ok(new {Message = "success"});
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Delete section
        [HttpPost]
        [Route("~/DeleteSection")]
        public async Task<IActionResult> DeleteSection(SectionForCudDto dto)
        {
            try
            {
                var find = sectionRep.GetById(dto.Id);
                if (find == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Code = "404",
                        Status = "Not Found",
                        Message = "not success",
                        Count = 1,
                        Error = "Section not Found"
                    });
                }
                
                sectionRep.Delete(find);
                await hub.Clients.All.SendAsync("AddSection");
                return Ok(new ApiResponse<Section>
                {
                    Message = "success",
                    Code = "200",
                    Status = "Ok",
                    Count = 1,
                    Data = find,
                });
            }
            catch (Exception)
            {
                return Ok(new ApiResponse<string>
                {
                    Message = "not success",
                    Code = "404",
                    Status = "Not Found",
                    Count = 0,
                    Error = "error",
                });

            }

        }
        #endregion

        #region GetDoctors
        [HttpGet]
        [Route("~/GetDoctors")]
        public async Task<IActionResult> GetDoctors()
        {
            try
            {
                var users = await userManager.Users.Where(u => u.Status == "Doctor").ToListAsync();
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



        #region GetAllRequests
        [HttpGet]
        [Route("~/getRequests")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var requests = await rep.GetAllAsync(a => true, new[] { "Patient" });
                var requestsToreturn = mapper.Map<IEnumerable<RequestForReturnDto>>(requests);
                return Ok(new ApiResponse<IEnumerable<RequestForReturnDto>>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = 1,
                    Data = requestsToreturn.OrderByDescending(r=>r.Created)
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region DeleteRequest
        [HttpPost]
        [Route("~/DeleteRequest")]
        public async Task<IActionResult> DeleteRequest(DeleteRequestDto dto)
        {
            try
            {
                var request = rep.GetById(dto.id);
                if (request == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Count = 0,
                        Message = "not success",
                        Error = "Request Not Found"
                    });
                }
                var result = rep.Delete(request);

                if (result != null)
                {
                    var user = await userManager.FindByIdAsync(result.PatientId);
                    if (dto.Status == 0)
                    {
                        user.Degree = "refuse";
                        rep.SaveAllAsync();
                    }
                    else
                    {
                        await userManager.RemoveFromRoleAsync(user, "Patient");
                        await userManager.AddToRoleAsync(user, "Doctor");
                        user.Status = "Doctor";
                        rep.SaveAllAsync();
                        await hub.Clients.All.SendAsync("EditUserRole");
                    }
                    await hub.Clients.All.SendAsync("DeleteRequests");
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 1,
                        Data = "Request was deleted"
                    });
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = "Ok",
                    Count = 0,
                    Message = "not succes",
                    Error = "Request already ordered"
                });
            }
        }
        #endregion

    }
}
