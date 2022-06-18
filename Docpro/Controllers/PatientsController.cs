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
using System.Security.Claims;

namespace Docpro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient,Admin")]
    public class PatientsController : ControllerBase
    {
        #region field
        private readonly IDynamicRep<Book> bookRep;
        private readonly IDynamicRep<Request> rep;
        private readonly IDynamicRep<Post> postRep;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHubContext<NotifyHub> hub;
        #endregion

        #region CTOR
        public PatientsController(IDynamicRep<Book> bookRep, IDynamicRep<Request> rep,IDynamicRep<Post> postRep, IMapper mapper,UserManager<ApplicationUser> userManager , IHubContext<NotifyHub> hub)
        {
            this.bookRep = bookRep;
            this.rep = rep;
            this.postRep = postRep;
            this.mapper = mapper;
            this.userManager = userManager;
            this.hub = hub;
        }
        #endregion


       

        #region MakeRequest
        [HttpPost]
        [Route("~/MakeRequest")]
        public async Task<IActionResult> MakeRequest(CreateRequestDto dto)
        {
            try
            {
                var patient = await userManager.FindByIdAsync(dto.PatientId);
                if (patient == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Count = 0,
                        Message = "not succes",
                        Error = "User Not Found"
                    });
                }
                var request = mapper.Map<Request>(dto);
                var result = rep.Add(request);

                if (result != null)
                {
                    patient.Degree = result.Degree;
                    rep.SaveAllAsync();
                    var returnResult = mapper.Map<RequestForReturnDto>(result);
                    await hub.Clients.All.SendAsync("makeRequest");
                    return Ok(new ApiResponse<RequestForReturnDto>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "Request success",
                        Count = 1,
                        Data = returnResult
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
                    Message = "not success",
                    Error = "Request already ordered"
                });
            }
        }
        #endregion


        #region Getposts
        [HttpGet]
        [Route("~/GetPosts")]
        public async Task<IActionResult> GetPosts()
        {
            try
            {
                var posts = await postRep.GetAllAsync(a => true, new[] { "Doctor" });
                var returnPosts = mapper.Map<IEnumerable<PostForReturnDto>>(posts);
                return Ok(new ApiResponse<IEnumerable<PostForReturnDto>>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = returnPosts.Count(a => true),
                    Data = returnPosts.OrderByDescending(a => a.Date).ToList()

                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #region BookDoctor
        [HttpPost]
        [Route("~/BookDoctor")]
        public async Task<IActionResult> BookDoctor(CreateBookDto dto)
        {
            try
            {
                var doctor = await userManager.FindByIdAsync(dto.DoctorId);
                var patient = await userManager.FindByIdAsync(dto.PatientId);
                if (doctor == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{dto.DoctorId}' didn't match any doctor ! "
                    });
                }

                if (patient == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{dto.PatientId}' didn't match any patient ! "
                    });
                }

                var book = mapper.Map<Book>(dto);
                var result = bookRep.Add(book);
                if (result == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"something error !"
                    });
                }

                await hub.Clients.All.SendAsync("BookDoctor");

                return Ok(new ApiResponse<CreateBookDto>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = 1,
                    Data = dto
                });
            }
            catch (Exception)
            {
                var user = await userManager.FindByIdAsync(dto.DoctorId);
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = 0,
                    Error = $"You have already booked with {user.FullName}"
                });
            }
        }
        #endregion


        #region GetAcceptedReservation
        [HttpGet]
        [Route("~/GetAcceptedReservation")]
        public async Task<IActionResult> GetAcceptedReservation()
        {
            try
            {
                var reservation = await bookRep.GetAllAsync(a => a.PatientId == User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Index == true && a.Status == "1", new[] { "Doctor" });
                var returnReservation = mapper.Map<IEnumerable<ReservationForReturnDto>>(reservation);
                return Ok(new ApiResponse<IEnumerable<ReservationForReturnDto>>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = returnReservation.Count(),
                    Data = returnReservation
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

    }
}
