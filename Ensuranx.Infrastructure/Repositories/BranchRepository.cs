using FirebaseAdmin.Auth;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Ensuranx.Infrastructure.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly ApplicationDbContext _context;
        private IUnitOfWork<long> _iunitOfWork;
        public BranchRepository(ApplicationDbContext context)
        {
            _context = context;
            _iunitOfWork = new UnitOfWork<long>(context);
        }

        public async Task<List<BranchScheduling>> GetBranchSchedulingList(long branchId)
        {
            var response = await _context.BranchScheduling.AsNoTracking().Where(x => x.BranchId == branchId).ToListAsync();
            return response;
        }
        public async Task<List<BranchSchedulingDetailByBranchId>> GetBranchSchedulingListById(long branchId)
        {
            var response = await _context.BranchScheduling.AsNoTracking().Where(x => x.BranchId == branchId)
                                .Select(x => new BranchSchedulingDetailByBranchId
                                {
                                    BranchId = x.BranchId,
                                    StartTime = x.StartTime,
                                    EndTime = x.EndTime,
                                    WeekDay = (long)x.WeekDay
                                }).ToListAsync();
            return response;
        }

        public async Task<List<Branch>> GetBranchByUserId(long userId)
        {
            return await _context.Branch.Where(x => x.UserInfoId == userId).ToListAsync();
        }

        public async Task<List<AllBranchesResponse>> GetAllBranchesList(long userId)
        {
            var results = await (from branch in _context.Branch.Where(x=>x.IsDeleted!=true && x.IsActive!=false)

                                 where branch.UserInfoId == userId

                                 join imageTbl in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Branch.ToString().ToLower() &&
                                          x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                 on branch.Id equals imageTbl.TableId into newFullImage
                                 from imageTblFull in newFullImage.DefaultIfEmpty()

                                 join imageTblCover in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Branch.ToString().ToLower() &&
                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                 on branch.Id equals imageTblCover.TableId into newFullImageCover
                                 from imageTblFullCover in newFullImageCover.DefaultIfEmpty()

                                 select new AllBranchesResponse
                                 {
                                     Id = branch.Id,
                                     Name = branch.Name,
                                     ProfileImageUrl = imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     CoverImageUrl = imageTblFullCover != null ? imageTblFullCover.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                 }
                                ).ToListAsync();
            return results;
        }

        public Task<List<GenericObj>> GetAllBranchesListByID(long userId)
        {
            return _context.Branch.Where(x=>x.UserInfoId==userId && x.IsDeleted!=true)
                                  .Select(x => new GenericObj { Id = x.Id, Name = x.Name })
                                  .ToListAsync();
        }
        public async Task<List<MatchesByIdRes>> GetMatchesByIdRepo(long eventCollectionId, long userId)
        {
            var results = await (from eventCollection in _context.EventCollection.Where(x=>x.Id ==  eventCollectionId )
                                 .Include(x=>x.Activity)
                                 .Include(x=>x.Venue)
                                 .Include(x=>x.UserInfo)
                                 select new MatchesByIdRes
                                 {
                                          Id = eventCollection.Id,
                                          venueId = eventCollection.VenueId,
                                          statKeeperName = eventCollection.UserInfo.Name,
                                          activity = eventCollection.Activity.Name,
                                 }).ToListAsync();

            return results;
        }

        public async Task<List<StatkeeperImagesDetails>> GetStatkeeeperDetails(List<long> branchId)
        {
            var results = await (
                        from branch in _context.Branch
                        join venue in _context.Venue on branch.Id equals venue.BranchId
                        where branchId.Contains(branch.Id)

                        join venueStatkeeper in _context.VenueStatkeeper on venue.Id equals venueStatkeeper.VenueId
                        join userInfo in _context.UserInfo on venueStatkeeper.UserInfoId equals userInfo.Id


                        join imageTbl in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on venueStatkeeper.UserInfoId equals imageTbl.TableId into newFullImage
                        from imageTblFull in newFullImage.DefaultIfEmpty()


                        select new StatkeeperImagesDetails
                        {
                            BranchId = branch.Id,
                            Id = (long)venueStatkeeper.UserInfoId,
                            Name = userInfo.Name,
                            ProfileImage= imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                        }
                    ).Distinct().ToListAsync();

            return results;
        }

        public async Task<long> getTotalVenueByBranch(long branchId)
        {
            var results = await _context.Venue.Where(x => x.BranchId == branchId).CountAsync();
            return results;
        }


        public async Task<long> getTotalSportByBranch(long branchId)
        {
            var results = await _context.VenueActivity.Where(x => x.Venue.BranchId == branchId).Select(x => x.ActivityId).Distinct().CountAsync();
            return results;
        }

        public async Task<long> getTotalMatchByBranch(long branchId)
        {
            var results = await (from ec in _context.EventCollection.Where(x => x.Venue.BranchId == branchId)
                                 join eventCollectionteam in _context.EventCollectionTeam
                                 on ec.Id equals eventCollectionteam.EventCollectionId

                                 join eventTeam in _context.EventTeam
                                 on eventCollectionteam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id

                                 select events.Id
                                 ).Distinct().CountAsync();

            return results;
        }

        public async Task<Ensuranx.Domain.Entities.Branch> UpdateBranch(Ensuranx.Domain.Entities.Branch branch)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.Branch>().UpdateAsync(branch);
            await _iunitOfWork.Commit(cancellationToken);
            return branch;
        }

        public async Task<BranchDetailByBranchId> getBranchDetail(long branchId)
        {
            var results = await (from branch in _context.Branch.Where(x => x.Id == branchId)
                                 join branchSchedul in _context.BranchScheduling

                                 on branch.Id equals branchSchedul.BranchId

                                 join imageTbl in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Branch.ToString().ToLower() &&
                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on branchId equals imageTbl.TableId into newFullImage
                                 from imageTblFull in newFullImage.DefaultIfEmpty()

                                 join imageTblCover in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Branch.ToString().ToLower() &&
                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())
                                 on branch.Id equals imageTblCover.TableId into newFullImageCover
                                 from imageTblFullCover in newFullImageCover.DefaultIfEmpty()

                                 select new BranchDetailByBranchId
                                 {
                                     Id = branchId,
                                     Name = branch.Name,
                                     ProfileImageUrl = imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     CoverImageUrl = imageTblFullCover != null ? imageTblFullCover.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                 }).FirstOrDefaultAsync();

            return results;

        }

        public async Task<List<ConnectionDetailsbyBranchId>> connectionDetailsbyBranchIds(long branchId)
        {
            var result = await (from connection in _context.Connection
                                where connection.TableId == branchId
                                join connectionEntity in _context.ConnectionEntity
                                on connection.ConnectionEntityId equals connectionEntity.Id

                                join connectiontype in _context.ConnectionType
                                on connection.ConnectionTypeId equals connectiontype.Id

                                select new ConnectionDetailsbyBranchId
                                {
                                    Name = connectiontype.Name,
                                    Value = connection.Value
                                }).ToListAsync();

            return result;

        }
        /*  public async Task<EventByUserId> GetAllEvents(long userId)
          {
              var results = await (from events in _context.Event.AsNoTracking()
                                   join evenTeam in _context.EventTeam.AsNoTracking()
                                   on events.Id equals evenTeam.EventId

                                   join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                   on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                   join team in _context.Team
                                   on eventTeamCollection.TeamId equals team.Id

                                   join teamMember in _context.TeamMember
                                   on team.Id equals teamMember.TeamId

                                   where teamMember.PlayerId == userId
                                   select new EventByUserId
                                   {
                                       eventId=events.Id,
                                       FirstTeamId=E


                                   }
                                   );
          }*/
    }
}
