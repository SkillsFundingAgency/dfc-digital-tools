using AutoMapper;
using DFC.Digital.Tools.Data.Models;
using System;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public class AccountsMapper : Profile
    {
        public AccountsMapper()
        {
            CreateMap<Accounts, AccountNotification>()
           .ForMember(a => a.Email, m => m.MapFrom(n => n.Mail))
           .ForMember(a => a.Name, m => m.MapFrom(n => n.Name));
        }
    }
}
