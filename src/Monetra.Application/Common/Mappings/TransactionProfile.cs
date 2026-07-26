using AutoMapper;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;

namespace Monetra.Application.Common.Mappings;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.TransactionType.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(s => s.PaymentMethod.HasValue ? s.PaymentMethod.Value.ToString() : null))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.BankAccount != null ? s.BankAccount.Name : null));

        CreateMap<BankAccount, BankAccountDto>()
            .ForMember(d => d.AccountType, opt => opt.MapFrom(s => s.AccountType.ToString()));

        CreateMap<TransactionCategory, CategoryDto>()
            .ForMember(d => d.TransactionType, opt => opt.MapFrom(s => s.TransactionType.ToString()));

        CreateMap<Wallet, WalletDto>()
            .ForMember(d => d.WalletType, opt => opt.MapFrom(s => s.WalletType.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ProgressPercentage, opt => opt.MapFrom(s => s.GetProgressPercentage()));

        CreateMap<WalletTransaction, WalletTransactionDto>();
    }
}
