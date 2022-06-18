using AutoMapper;
using Docpro.BL.Dtos;
using Docpro.BL.Helper;
using Docpro.BL.Interface;
using Docpro.DAL.Entity;
using Docpro.DAL.extend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Docpro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Doctor,Admin")]
    public class DoctorsController : ControllerBase
    {
        #region fields
        private readonly IDynamicRep<AvailableTimes> timeRep;
        private readonly IDynamicRep<BooKRepot> reportRep;
        private readonly IDynamicRep<Post> postRep;
        private readonly IDynamicRep<Book> bookRep;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHubContext<NotifyHub> hub;
        #endregion


        #region Ctor
        public DoctorsController(IDynamicRep<AvailableTimes> timeRep, IDynamicRep<BooKRepot> ReportRep, IDynamicRep<Post> postRep,IDynamicRep<Book> bookRep, IMapper mapper,UserManager<ApplicationUser> userManager, IHubContext<NotifyHub> hub)
        {
            this.timeRep = timeRep;
            reportRep = ReportRep;
            this.postRep = postRep;
            this.bookRep = bookRep;
            this.mapper = mapper;
            this.userManager = userManager;
            this.hub = hub;
        }
        #endregion


        #region Actions

        #region GetPosts
        [HttpGet]
        [Route("~/GetDoctorPosts")]
        public async Task<IActionResult> GetDoctorPosts()
        {
            try
            {
                var doctorId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var posts = await postRep.GetAllAsync(a => a.DoctorId == doctorId, new[] { "Doctor" });
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

        #region AddPost
        [HttpPost]
        [Route("~/AddPost")]
        public async Task<IActionResult> AddPost([FromBody] CreatePostDto dto)
        {
            try
            {
                var doctor = await userManager.FindByIdAsync(dto.DoctorId);
                if (doctor == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 1,
                        Error = "This id don't match any doctor ",
                    });
                }
                var post = mapper.Map<Post>(dto);
                var result = postRep.Add(post);
                if (result == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = "Somthing Error !"
                    });
                }
                await hub.Clients.All.SendAsync("postAction");
                var returnPost = mapper.Map<PostForReturnDto>(post);
                return Ok(new ApiResponse<PostForReturnDto>
                {
                    Code = "200",
                    Status = "Ok",
                    Message = "success",
                    Count = 1,
                    Data = returnPost,
                });
                
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region DeletePost
        [HttpPost]
        [Route("~/DeletePost")]
        public async Task<IActionResult> DeletePost(DeleteEditPostDto dto)
        {

            try
            {
                var find = postRep.GetById(dto.Id);
                if (find == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code="200",
                        Status = " Ok",
                        Message = "success",
                        Count = 0,
                        Error="No post match with this id"
                    });
                }
                var result = postRep.Delete(find);
                if (result == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 0,
                        Error = "Something error!"
                    });
                }
                await hub.Clients.All.SendAsync("postAction");
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = 0,
                    Data = "Deleted Done !"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region addTime
        [HttpPost]
        [Route("~/AddAvailableTime")]
        public async Task<IActionResult> AddAvailableTime([FromBody] CreateAvailabletimeDTO dTO)
        {
            try
            {
                var user = await userManager.FindByIdAsync(dTO.doctorId);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{dTO.doctorId}' don't match any doctor !"
                    });
                }
                var time = mapper.Map<AvailableTimes>(dTO);
                var result = timeRep.Add(time);
                if (result != null)
                {
                    await hub.Clients.All.SendAsync("AddAvailableTimes");
                    return Ok(new ApiResponse<CreateAvailabletimeDTO>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 1,
                        Data = dTO
                    });
                }
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = 0,
                    Error = "Something Error !"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region editTime
        [HttpPut]
        [Route("~/EditAvailableTime")]
        public async Task<IActionResult> EditAvailableTime([FromBody] EditAndDeleteAvailableTimeDto dTO)
        {
            try
            {
                var user = await userManager.FindByIdAsync(dTO.doctorId);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{dTO.doctorId}' don't match any doctor !"
                    });
                }
                var time = mapper.Map<AvailableTimes>(dTO);
                var result = timeRep.Edit(time);
                if (result != null)
                {
                    await hub.Clients.All.SendAsync("EditAvailableTimes");
                    return Ok(new ApiResponse<EditAndDeleteAvailableTimeDto>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 1,
                        Data = dTO
                    });
                }
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = 0,
                    Error = "Something Error !"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region DeleteTime
        [HttpPost]
        [Route("~/DeleteAvailableTime")]
        public async Task<IActionResult> DeleteAvailableTime([FromBody] EditAndDeleteAvailableTimeDto dTO)
        {
            try
            {
                if (User.FindFirst(ClaimTypes.NameIdentifier).Value != dTO.doctorId)
                    return Unauthorized();
                var time = mapper.Map<AvailableTimes>(dTO);
                var result = timeRep.Delete(time);
                if (result != null)
                {
                    await hub.Clients.All.SendAsync("DeleteAvailableTimes");
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = " Ok",
                        Message = "success",
                        Count = 1,
                        Data = "Deleted Done !"
                    });
                }
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = 0,
                    Error = "Something Error !"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region GetReservation
        [HttpGet]
        [Route("~/GetReservation")]
        public async Task<IActionResult> GetReservation()
        {
            try
            {
                var reservation = await bookRep.GetAllAsync(a => a.DoctorId == User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Index == false, new[] { "Patient" });
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

        #region GetWorkSchedule
        [HttpGet]
        [Route("~/GetWorkSchedule")]
        public async Task<IActionResult> GetWorkSchedule()
        {
            try
            {
                var reservation = await bookRep.GetAllAsync(a => a.DoctorId == User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Index == true&&a.Status != "2", new[] { "Patient" });
                var returnReservation = mapper.Map<IEnumerable<ReservationForReturnDto>>(reservation);
                return Ok(new ApiResponse<IEnumerable<ReservationForReturnDto>>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Count = returnReservation.Count(),
                    Data = returnReservation.OrderBy(a => a.Date)
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region confirmReservation
        [HttpPost]
        [Route("~/confirmReservation/{patientId}")]
        public async Task<IActionResult> confirmReservation(string patientId)
        {
            try
            {
                var patient = await userManager.FindByIdAsync(patientId);

                if (patient == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{patientId}' didn't match any patient ! "
                    });
                }
                var book = await bookRep.GetByIdAsync(b => b.DoctorId == User.FindFirst(ClaimTypes.NameIdentifier).Value && b.PatientId == patientId);
                book.Index = true;
                book.Status = "1";
                bookRep.Edit(book);
                
                await hub.Clients.All.SendAsync("confirmReservations");
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Data = "confirmed success"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region RefuseReservation
        [HttpPost]
        [Route("~/RefuseReservation/{patientId}")]
        public async Task<IActionResult> RefuseReservation(string patientId)
        {
            try
            {
                var patient = await userManager.FindByIdAsync(patientId);

                if (patient == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{patientId}' didn't match any patient ! "
                    });
                }
                var book = await bookRep.GetByIdAsync(b => b.DoctorId == User.FindFirst(ClaimTypes.NameIdentifier).Value && b.PatientId == patientId);
                bookRep.Delete(book);
                await hub.Clients.All.SendAsync("RefuseReservations");
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Data = "Refuse success"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region ReservationDone
        [HttpPost]
        [Route("~/ReservationDone/{patientId}")]
        public async Task<IActionResult> ReservationDone(string patientId,CreateReportDto reportDto)
        {
            try
            {
                var patient = await userManager.FindByIdAsync(patientId);

                if (patient == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Code = "200",
                        Status = "Ok",
                        Message = "success",
                        Count = 0,
                        Error = $"this id '{patientId}' didn't match any patient ! "
                    });
                }
                var book = await bookRep.GetByIdAsync(b => b.DoctorId == User.FindFirst(ClaimTypes.NameIdentifier).Value && b.PatientId == patientId);
                book.Status = "2";

                var report = new BooKRepot
                {
                    Date = DateTime.Now,
                    PatientId = book.PatientId,
                    DoctorId = book.DoctorId,
                    Day = book.Day,
                    From = book.From,
                    To = book.To,
                    Index = book.Index,
                    Status = book.Status,
                    Diagnosis = reportDto.Diagnosis,
                    treatment = reportDto.treatment
                };
                reportRep.Add(report);
                bookRep.Delete(book);
                await hub.Clients.All.SendAsync("ReservationDone");
                return Ok(new ApiResponse<string>
                {
                    Code = "200",
                    Status = " Ok",
                    Message = "success",
                    Data = "Reservation Done"
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        

        #endregion
    }
}
