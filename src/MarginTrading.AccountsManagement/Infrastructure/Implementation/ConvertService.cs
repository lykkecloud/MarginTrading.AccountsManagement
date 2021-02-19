// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using AutoMapper;
using Common;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Models.AdditionalInfo;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AssetService.Contracts.TradingConditions;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.Infrastructure.Implementation
{
    [UsedImplicitly]
    internal class ConvertService : IConvertService
    {
        private readonly IMapper _mapper = CreateMapper();

        private static IMapper CreateMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                // todo: add some global configurations here?
                cfg.CreateMap<AccountBalanceChangeReasonType, string>().ConvertUsing(x => x.ToString());
                cfg.CreateMap<string, AccountBalanceChangeReasonType>()
                    .ConvertUsing(Enum.Parse<AccountBalanceChangeReasonType>);
                cfg.CreateMap<List<string>, string>().ConvertUsing(JsonConvert.SerializeObject);
                cfg.CreateMap<string, List<string>>().ConvertUsing(JsonConvert.DeserializeObject<List<string>>);
                cfg.CreateMap<IAccount, AccountContract>()
                    .ForMember(p => p.AdditionalInfo,
                        s => s.ResolveUsing(x => x.AdditionalInfo.Serialize()));
                cfg.CreateMap<IClient, ClientTradingConditionsContract>().ForMember(x => x.ClientId, o => o.MapFrom(s=> s.Id));
            }).CreateMapper();
        }

        public TResult Convert<TSource, TResult>(TSource source,
            Action<IMappingOperationOptions<TSource, TResult>> opts)
        {
            return _mapper.Map(source, opts);
        }

        public TResult Convert<TSource, TResult>(TSource source)
        {
            return _mapper.Map<TSource, TResult>(source);
        }

        public TResult Convert<TResult>(object source)
        {
            return _mapper.Map<TResult>(source);
        }
    }
}