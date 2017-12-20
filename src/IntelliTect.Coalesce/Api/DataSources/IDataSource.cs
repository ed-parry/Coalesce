﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce
{

    public interface IDataSource<T> : IAuthorizable
        where T : class, new()
    {
        Task<(T Item, IncludeTree IncludeTree)> GetItemAsync(object id, IDataSourceParameters parameters);
        Task<TDto> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : IClassDto<T>, new();

        Task<(ListResult<T> List, IncludeTree IncludeTree)> GetListAsync(IListParameters parameters);
        Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : IClassDto<T>, new();

        Task<int> GetCountAsync(IFilterParameters parameters);
    }
}