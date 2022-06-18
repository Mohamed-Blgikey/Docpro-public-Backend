using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docpro.DAL.extend;
using Docpro.BL.Dtos;
using Docpro.DAL.Entity;

namespace Docpro.BL.Helper
{
    public class DomainProfile:Profile
    {
        public DomainProfile()
        {
            CreateMap<ApplicationUser, UserForReturnDto>();
            CreateMap<Section, SectionForReturnDto>();

            CreateMap<SectionForCudDto, Section>();

            CreateMap<CreateRequestDto, Request>();
            CreateMap<Request,RequestForReturnDto>();


            CreateMap<CreatePostDto, Post>();
            CreateMap<Post, PostForReturnDto>();

            CreateMap<CreateAvailabletimeDTO, AvailableTimes>();
            CreateMap<EditAndDeleteAvailableTimeDto, AvailableTimes>();

            CreateMap<CreateBookDto, Book>();
            CreateMap<Book, ReservationForReturnDto>();

            CreateMap<BooKRepot, ReportForReturnDto>();

        }
    }
}
