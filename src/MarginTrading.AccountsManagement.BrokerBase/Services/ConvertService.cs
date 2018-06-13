using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using JetBrains.Annotations;
using MoreLinq;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.BrokerBase.Services
{
    [UsedImplicitly]
    public class ConvertService : IConvertService
    {
        public ConvertService(Action<IMapperConfigurationExpression> mapperConfig = null)
        {
            _mapper = CreateMapper(mapperConfig);
        }
        
        private readonly IMapper _mapper;

        private static IMapper CreateMapper(Action<IMapperConfigurationExpression> mapperConfig = null)
        {
            return new MapperConfiguration(cfg =>
            {
                // todo: add some global configurations here?
                mapperConfig?.Invoke(cfg);
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