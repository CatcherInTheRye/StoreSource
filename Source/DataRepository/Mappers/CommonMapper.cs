using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using DataRepository;
using DataRepository.DataContracts;

namespace PCSMvc.Mappers
{
    public class CommonMapper : IMapper
    {
        static CommonMapper()
        {
            Mapper.CreateMap<user, UserForm>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.active))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.firstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.lastName))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.middleName))
                .ForMember(dest => dest.Salutation, opt => opt.MapFrom(src => src.salutation))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.phone))
                .ForMember(dest => dest.Cell, opt => opt.MapFrom(src => src.cell));
            Mapper.CreateMap<UserForm, user>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.active, opt => opt.MapFrom(src => src.Active))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.firstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.lastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.middleName, opt => opt.MapFrom(src => src.MiddleName))
                .ForMember(dest => dest.salutation, opt => opt.MapFrom(src => src.Salutation))
                .ForMember(dest => dest.phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.cell, opt => opt.MapFrom(src => src.Cell));



            Mapper.CreateMap<organizationSetting, OrganizationSettingsForm>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.organizationId))
                .ForMember(dest => dest.Sync, opt => opt.MapFrom(src => src.sync))
                .ForMember(dest => dest.PowerSchoolUrl, opt => opt.MapFrom(src => src.powerSchoolUrl))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.clientId))
                .ForMember(dest => dest.ClientSecret, opt => opt.MapFrom(src => src.clientSecret));

            Mapper.CreateMap<OrganizationSettingsForm, organizationSetting>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.organizationId, opt => opt.MapFrom(src => src.OrganizationId))
                .ForMember(dest => dest.sync, opt => opt.MapFrom(src => src.Sync))
                .ForMember(dest => dest.powerSchoolUrl, opt => opt.MapFrom(src => src.PowerSchoolUrl))
                .ForMember(dest => dest.clientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.clientSecret, opt => opt.MapFrom(src => src.ClientSecret));
        }

        public object Map(object source, Type sourceType, Type destinationType)
        {
            return Mapper.Map(source, sourceType, destinationType);
        }
    }
}