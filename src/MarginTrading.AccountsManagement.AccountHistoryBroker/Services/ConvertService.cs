﻿using System;
using System.Collections.Generic;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Services
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