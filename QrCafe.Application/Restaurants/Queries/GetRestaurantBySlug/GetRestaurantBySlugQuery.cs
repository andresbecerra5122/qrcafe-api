using System;
using MediatR;

namespace QrCafe.Application.Restaurants.Queries.GetRestaurantBySlug
{
    public record GetRestaurantBySlugQuery(string Slug) : IRequest<GetRestaurantBySlugResult?>;
}
