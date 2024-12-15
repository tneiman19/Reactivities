using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Followers
{
    public class List
    {
        public class Query : IRequest<Result<List<Profiles.Profile>>>
        {
            public string Predicate { get; set; }
            public string Username { get; set; }
        }

        public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            : IRequestHandler<Query, Result<List<Profiles.Profile>>>
        {
            public async Task<Result<List<Profiles.Profile>>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                var profiles = new List<Profiles.Profile>();

                switch (request.Predicate)
                {
                    case "followers":
                        profiles = await context
                            .UserFollowings.Where(x => x.Target.UserName == request.Username)
                            .Select(u => u.Observer)
                            .ProjectTo<Profiles.Profile>(
                                mapper.ConfigurationProvider,
                                new { currentUserName = userAccessor.GetUsername() }
                            )
                            .ToListAsync(cancellationToken: cancellationToken);
                        break;
                    case "following":
                        profiles = await context
                            .UserFollowings.Where(x => x.Observer.UserName == request.Username)
                            .Select(u => u.Target)
                            .ProjectTo<Profiles.Profile>(
                                mapper.ConfigurationProvider,
                                new { currentUserName = userAccessor.GetUsername() }
                            )
                            .ToListAsync(cancellationToken: cancellationToken);
                        break;
                }
                return Result<List<Profiles.Profile>>.Success(profiles);
            }
        }
    }
}
