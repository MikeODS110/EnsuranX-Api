using ErrorOr;
using Google.Apis.Util;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Tournament;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Constant;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.ExtensionMethods;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Tournament> GetTournamentByIdAsync(long id, long userId)
        {
            return await _context.Tournament.AsNoTracking().Where(x => x.Id == id).FirstAsync();
        }

        public IQueryable<Domain.Entities.Tournament> GetAll(long activityCategoryId)
        {
            var results = _context.Tournament
                .Where(x => activityCategoryId != 0 ? x.Activity.ActivityCategory.Id == activityCategoryId : x.Id != 0)
                .AsNoTracking().AsQueryable();
            return results;
        }

        //public IQueryable<Domain.Entities.Tournament> SearchInTournament(string sSearch)
        //{


        //    var tournaments = _context.Tournament
        //        .Where(x => !string.IsNullOrEmpty(sSearch) ? x.Name.ToLower().Contains(sSearch.Trim().ToLower()) : x.Id != 0)
        //        .Select(tournament => new Domain.Entities.Tournament
        //        {
        //            Id = tournament.Id,
        //            Name = tournament.Name,
        //            ActivityId = tournament.ActivityId,
        //            StartDateTime = tournament.StartDateTime,
        //            EndDateTime = tournament.EndDateTime,
        //            FirstPrize = tournament.FirstPrize,
        //            SecondPrize = tournament.SecondPrize,
        //            ThirdPrize = tournament.ThirdPrize,
        //            TeamCapacity = tournament.TeamCapacity,
        //            MinPlayerPerTeam = tournament.MinPlayerPerTeam,
        //            MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
        //            CreatedDateTime = tournament.CreatedDateTime,
        //            LastModifiedDateTime = tournament.LastModifiedDateTime,
        //            CreatedBy = tournament.CreatedBy,
        //            LastModifiedBy = tournament.LastModifiedBy,
        //            IsDeleted = tournament.IsDeleted,
        //            IsActive = tournament.IsActive,
        //            TournamentTypeId = tournament.TournamentTypeId,
        //            SeasonEndDate = tournament.SeasonEndDate,
        //            SeasonStartDate = tournament.SeasonStartDate,
        //            SeasonTeamCapacity = tournament.SeasonTeamCapacity

        //        });

        //    if (tournaments.Count() == 0)
        //    {
        //        var results = (from venue in _context.Venue
        //                       join tournamentVenue in _context.TournamentVenue
        //                       on venue.Id equals tournamentVenue.VenueId
        //                       join tournament in _context.Tournament
        //                       on tournamentVenue.TournamentId equals tournament.Id
        //                       where venue.Branch.Name.Contains(sSearch.Trim().ToLower())
        //                       select new Domain.Entities.Tournament
        //                       {
        //                           Id = tournament.Id,
        //                           Name = tournament.Name,
        //                           ActivityId = tournament.ActivityId,
        //                           StartDateTime = tournament.StartDateTime,
        //                           EndDateTime = tournament.EndDateTime,
        //                           FirstPrize = tournament.FirstPrize,
        //                           SecondPrize = tournament.SecondPrize,
        //                           ThirdPrize = tournament.ThirdPrize,
        //                           TeamCapacity = tournament.TeamCapacity,
        //                           MinPlayerPerTeam = tournament.MinPlayerPerTeam,
        //                           MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
        //                           CreatedDateTime = tournament.CreatedDateTime,
        //                           LastModifiedDateTime = tournament.LastModifiedDateTime,
        //                           CreatedBy = tournament.CreatedBy,
        //                           LastModifiedBy = tournament.LastModifiedBy,
        //                           IsDeleted = tournament.IsDeleted,
        //                           IsActive = tournament.IsActive,
        //                           TournamentTypeId = tournament.TournamentTypeId,
        //                           SeasonEndDate = tournament.SeasonEndDate,
        //                           SeasonStartDate = tournament.SeasonStartDate,
        //                           SeasonTeamCapacity = tournament.SeasonTeamCapacity
        //                       }).Distinct();
        //        if (results.Count() == 0)
        //        {
        //            var resultsStatkeeper = (from tournament in _context.Tournament
        //                                     join tournamentStatkeeper in _context.TournamentStatkeeper
        //                                     on tournament.Id equals tournamentStatkeeper.TournamentId into statkeepers
        //                                     from statkeeper in statkeepers.DefaultIfEmpty()
        //                                     join userInfo in _context.UserInfo
        //                                     on statkeeper.StatkeeperId equals userInfo.Id
        //                                     where tournament.Name.Contains(sSearch.Trim().ToLower()) ||
        //                                           userInfo.Name.Contains(sSearch.Trim().ToLower())
        //                                     select new Domain.Entities.Tournament
        //                                     {
        //                                         Id = tournament.Id,
        //                                         Name = tournament.Name,
        //                                         ActivityId = tournament.ActivityId,
        //                                         StartDateTime = tournament.StartDateTime,
        //                                         EndDateTime = tournament.EndDateTime,
        //                                         FirstPrize = tournament.FirstPrize,
        //                                         SecondPrize = tournament.SecondPrize,
        //                                         ThirdPrize = tournament.ThirdPrize,
        //                                         TeamCapacity = tournament.TeamCapacity,
        //                                         MinPlayerPerTeam = tournament.MinPlayerPerTeam,
        //                                         MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
        //                                         CreatedDateTime = tournament.CreatedDateTime,
        //                                         LastModifiedDateTime = tournament.LastModifiedDateTime,
        //                                         CreatedBy = tournament.CreatedBy,
        //                                         LastModifiedBy = tournament.LastModifiedBy,
        //                                         IsDeleted = tournament.IsDeleted,
        //                                         IsActive = tournament.IsActive,
        //                                         TournamentTypeId = tournament.TournamentTypeId,
        //                                         SeasonEndDate = tournament.SeasonEndDate,
        //                                         SeasonStartDate = tournament.SeasonStartDate,
        //                                         SeasonTeamCapacity = tournament.SeasonTeamCapacity
        //                                     });

        //            return resultsStatkeeper.AsQueryable();
        //        }
        //        return results.AsQueryable();
        //    }
        //    return tournaments.AsQueryable();


        //}
        public IQueryable<Domain.Entities.Tournament> SearchInTournament(string sSearch)
        {
            var tournaments = _context.Tournament
                   .Where(x => !string.IsNullOrEmpty(sSearch) ? x.Name.ToLower().Contains(sSearch.Trim().ToLower()) : x.Id != 0)
                   .Select(tournament => new Domain.Entities.Tournament
                   {
                       Id = tournament.Id,
                       Name = tournament.Name,
                       ActivityId = tournament.ActivityId,
                       StartDateTime = tournament.StartDateTime,
                       EndDateTime = tournament.EndDateTime,
                       FirstPrize = tournament.FirstPrize,
                       SecondPrize = tournament.SecondPrize,
                       ThirdPrize = tournament.ThirdPrize,
                       TeamCapacity = tournament.TeamCapacity,
                       MinPlayerPerTeam = tournament.MinPlayerPerTeam,
                       MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
                       CreatedDateTime = tournament.CreatedDateTime,
                       LastModifiedDateTime = tournament.LastModifiedDateTime,
                       CreatedBy = tournament.CreatedBy,
                       LastModifiedBy = tournament.LastModifiedBy,
                       IsDeleted = tournament.IsDeleted,
                       IsActive = tournament.IsActive,
                       TournamentTypeId = tournament.TournamentTypeId,
                       SeasonEndDate = tournament.SeasonEndDate,
                       SeasonStartDate = tournament.SeasonStartDate,
                       SeasonTeamCapacity = tournament.SeasonTeamCapacity

                   });

            var results = (from venue in _context.Venue
                           join tournamentVenue in _context.TournamentVenue
                           on venue.Id equals tournamentVenue.VenueId
                           join tournament in _context.Tournament
                           on tournamentVenue.TournamentId equals tournament.Id
                           where venue.Branch.Name.Contains(sSearch.Trim().ToLower())
                           select new Domain.Entities.Tournament
                           {
                               Id = tournament.Id,
                               Name = tournament.Name,
                               ActivityId = tournament.ActivityId,
                               StartDateTime = tournament.StartDateTime,
                               EndDateTime = tournament.EndDateTime,
                               FirstPrize = tournament.FirstPrize,
                               SecondPrize = tournament.SecondPrize,
                               ThirdPrize = tournament.ThirdPrize,
                               TeamCapacity = tournament.TeamCapacity,
                               MinPlayerPerTeam = tournament.MinPlayerPerTeam,
                               MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
                               CreatedDateTime = tournament.CreatedDateTime,
                               LastModifiedDateTime = tournament.LastModifiedDateTime,
                               CreatedBy = tournament.CreatedBy,
                               LastModifiedBy = tournament.LastModifiedBy,
                               IsDeleted = tournament.IsDeleted,
                               IsActive = tournament.IsActive,
                               TournamentTypeId = tournament.TournamentTypeId,
                               SeasonEndDate = tournament.SeasonEndDate,
                               SeasonStartDate = tournament.SeasonStartDate,
                               SeasonTeamCapacity = tournament.SeasonTeamCapacity
                           }).Distinct();

            //var resultsStatkeeper = (from tournament in _context.Tournament
            //                         join tournamentStatkeeper in _context.TournamentStatkeeper
            //                         on tournament.Id equals tournamentStatkeeper.TournamentId into statkeepers
            //                         from statkeeper in statkeepers.DefaultIfEmpty()
            //                         join userInfo in _context.UserInfo
            //                         on statkeeper.StatkeeperId equals userInfo.Id
            //                         where userInfo.Name.Contains(sSearch.Trim().ToLower())
            //                         select new Domain.Entities.Tournament
            //                         {
            //                             Id = tournament.Id,
            //                             Name = tournament.Name,
            //                             ActivityId = tournament.ActivityId,
            //                             StartDateTime = tournament.StartDateTime,
            //                             EndDateTime = tournament.EndDateTime,
            //                             FirstPrize = tournament.FirstPrize,
            //                             SecondPrize = tournament.SecondPrize,
            //                             ThirdPrize = tournament.ThirdPrize,
            //                             TeamCapacity = tournament.TeamCapacity,
            //                             MinPlayerPerTeam = tournament.MinPlayerPerTeam,
            //                             MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
            //                             CreatedDateTime = tournament.CreatedDateTime,
            //                             LastModifiedDateTime = tournament.LastModifiedDateTime,
            //                             CreatedBy = tournament.CreatedBy,
            //                             LastModifiedBy = tournament.LastModifiedBy,
            //                             IsDeleted = tournament.IsDeleted,
            //                             IsActive = tournament.IsActive,
            //                             TournamentTypeId = tournament.TournamentTypeId,
            //                             SeasonEndDate = tournament.SeasonEndDate,
            //                             SeasonStartDate = tournament.SeasonStartDate,
            //                             SeasonTeamCapacity = tournament.SeasonTeamCapacity
            //                         });

            var tournamentsall = _context.Tournament
               .Select(tournament => new Domain.Entities.Tournament
               {
                   Id = tournament.Id,
                   Name = tournament.Name,
                   ActivityId = tournament.ActivityId,
                   StartDateTime = tournament.StartDateTime,
                   EndDateTime = tournament.EndDateTime,
                   FirstPrize = tournament.FirstPrize,
                   SecondPrize = tournament.SecondPrize,
                   ThirdPrize = tournament.ThirdPrize,
                   TeamCapacity = tournament.TeamCapacity,
                   MinPlayerPerTeam = tournament.MinPlayerPerTeam,
                   MaxPlayerPerTeam = tournament.MaxPlayerPerTeam,
                   CreatedDateTime = tournament.CreatedDateTime,
                   LastModifiedDateTime = tournament.LastModifiedDateTime,
                   CreatedBy = tournament.CreatedBy,
                   LastModifiedBy = tournament.LastModifiedBy,
                   IsDeleted = tournament.IsDeleted,
                   IsActive = tournament.IsActive,
                   TournamentTypeId = tournament.TournamentTypeId,
                   SeasonEndDate = tournament.SeasonEndDate,
                   SeasonStartDate = tournament.SeasonStartDate,
                   SeasonTeamCapacity = tournament.SeasonTeamCapacity

               });

            return tournaments.Any() ? tournaments.AsQueryable() :
                    results.Any() ? results.AsQueryable() :
                    tournamentsall.AsQueryable();



        }


        //public Task<List<Domain.Entities.Tournament>> GetAll(long activityCategoryId, string sSearch)
        //{
        //    var results = _context.Tournament
        //        .Where(x => activityCategoryId != 0 ? x.Activity.ActivityCategory.Id == activityCategoryId : x.Id != 0)
        //        .Where(x => !string.IsNullOrEmpty(sSearch) ? x.Name.ToLower().Contains(sSearch.Trim().ToLower()) : x.Id != 0)
        //        .AsNoTracking().ToListAsync();


        //    var query = from tournament in _context.Tournament
        //                join tournamentStatkeepr in _context.TournamentStatkeeper
        //                on tournament.Id equals tournamentStatkeepr.TournamentId

        //                join userInfo in _context.UserInfo
        //                on tournamentStatkeepr.StatkeeperId equals userInfo.Id

        //                where userInfo.Name == sSearch

        //                select new Domain.Entities.Tournament
        //                {

        //                };
        //    //return (Task<List<Domain.Entities.Tournament>>)query;



        //    return results;

        //}

        public async Task<string> CheckForTournamentIsJoinedByUserAnyTeam(long tournamentId, long userId)
        {
            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                               join tournamentTeamTbl in _context.TournamentTeam.AsNoTracking()
                               on tournamentTbl.Id equals tournamentTeamTbl.TournamentId
                               join teamMemberTbl in _context.TeamMember.AsNoTracking().Include(x => x.TeamMemberRole)
                               on tournamentTeamTbl.TeamId equals teamMemberTbl.TeamId
                               where tournamentTbl.Id == tournamentId && teamMemberTbl.PlayerId == userId
                               //&& teamMemberTbl.TeamMemberRole.Name.ToLower() == Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower()

                               select tournamentTeamTbl
                               ).FirstOrDefaultAsync();
            return query != null ? query.IsAccepted != false ? Constant.Joined : Constant.REQUEST_PENDING : Constant.REQUEST_TO_JOIN;
        }

        public async Task<bool> CheckForTournamentIsCaptainByUserAnyTeam(long tournamentId, long userId)
        {
            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                               join tournamentTeamTbl in _context.TournamentTeam.AsNoTracking()
                               on tournamentTbl.Id equals tournamentTeamTbl.TournamentId
                               join teamMemberTbl in _context.TeamMember.AsNoTracking().Include(x => x.TeamMemberRole)
                               on tournamentTeamTbl.TeamId equals teamMemberTbl.TeamId
                               where tournamentTbl.Id == tournamentId && teamMemberTbl.PlayerId == userId
                               && teamMemberTbl.TeamMemberRole.Name.ToLower() == Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower()

                               select tournamentTeamTbl
                               ).FirstOrDefaultAsync();

            return query != null ? true : false;
        }

        public async Task<bool> GetTournamentFormatByTournamentId(long tournamentId, long userId)
        {
            var query = await (from tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                               join eventTeamTbl in _context.EventTeam.AsNoTracking()
                               on tournamentEventCollectionTbl.EventCollectionId equals eventTeamTbl.EventCollectionTeam.EventCollectionId

                               where tournamentEventCollectionTbl.TournamentId == tournamentId && eventTeamTbl.Round == 0
                               select eventTeamTbl
                               ).FirstOrDefaultAsync();
            //true means season is finished and false means season is still in progress
            return query != null ? true : false;
        }

        public async Task<GetTournament> GetTournamentByIdJoinAsync(long id)
        {
            //var getTournament = await (from Tournament in _context.Tournament.AsNoTracking()
            //                            .Where(x => x.Id == id)
            //                           .Include(x => x.Activity.ActivityCategory)
            //              join TournamentVenue in _context.TournamentVenue.AsNoTracking()
            //              .Include(x => x.Venue).
            //               Include(x => x.Venue.Branch)
            //              on Tournament.Id equals TournamentVenue.TournamentId

            //              join tournamentStatkeeper in _context.TournamentStatkeeper
            //              on Tournament.Id equals tournamentStatkeeper.TournamentId

            //              group TournamentVenue by TournamentVenue.TournamentId into g

            //              select new GetTournament
            //              {
            //                  Id = g.First().Tournament.Id,
            //                  TeamCapacity = g.First().Tournament.TeamCapacity,
            //                  ActivityName = g.First().Tournament.Activity.ActivityCategory.Name,
            //                  StartDateTime = g.First().Tournament.StartDateTime,
            //                  EndDateTime = g.First().Tournament.EndDateTime,
            //                  MinPlayerPerTeam = g.First().Tournament.MinPlayerPerTeam,
            //                  MaxPlayerPerTeam = g.First().Tournament.MaxPlayerPerTeam,
            //                  TeamJoinedCount = _context.TournamentTeam.AsNoTracking().Where(x => x.TournamentId == g.First().Tournament.Id && x.IsAccepted != false).Count(),
            //                  Name = g.First().Tournament.Name,
            //                  BranchName = g.First().Venue.Branch.Name,
            //                  Status = (DateTime.Compare(g.First().Tournament.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(g.First().Tournament.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(g.First().Tournament.EndDateTime, DateTime.UtcNow) > 0 ?
            //                               Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(g.First().Tournament.StartDateTime, DateTime.UtcNow) > 0 ?
            //                               Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
            //                               DateTime.Compare(g.First().Tournament.EndDateTime, DateTime.UtcNow) < 0 ?
            //                               Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
            //                  VenueList = g.Select(t => new GenericObj
            //                  {
            //                      Name = t.Venue.Name,
            //                      Id = t.Id,
            //                  }).ToList(),
            //              }).FirstOrDefaultAsync();
            var getTournament = await (from Tournament in _context.Tournament.AsNoTracking()
                                       .Where(x => x.Id == id)
                                       .Include(x => x.Activity.ActivityCategory)
                                       join TournamentVenue in _context.TournamentVenue.AsNoTracking()
                                       .Include(x => x.Venue).
                                        Include(x => x.Venue.Branch)
                                       on Tournament.Id equals TournamentVenue.TournamentId

                                       join tournamentStatkeeper in _context.TournamentStatkeeper
                                       on Tournament.Id equals tournamentStatkeeper.TournamentId

                                       select new GetTournament
                                       {
                                           Id = Tournament.Id,
                                           TeamCapacity = Tournament.TeamCapacity,
                                           ActivityName = Tournament.Activity.ActivityCategory.Name,
                                           ActivityId = Tournament.Activity.ActivityCategory.Id,
                                           StartDateTime = Tournament.StartDateTime,
                                           EndDateTime = Tournament.EndDateTime,
                                           MinPlayerPerTeam = Tournament.MinPlayerPerTeam,
                                           MaxPlayerPerTeam = Tournament.MaxPlayerPerTeam,
                                           TeamJoinedCount = _context.TournamentTeam.AsNoTracking().Where(x => x.TournamentId == Tournament.Id && x.IsAccepted != false).Count(),
                                           Name = Tournament.Name,

                                           Status = (DateTime.Compare(Tournament.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(Tournament.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(Tournament.EndDateTime, DateTime.UtcNow) > 0 ?
                                                        Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(Tournament.StartDateTime, DateTime.UtcNow) > 0 ?
                                                        Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
                                                        DateTime.Compare(Tournament.EndDateTime, DateTime.UtcNow) < 0 ?
                                                        Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                           VenueList = _context.TournamentVenue
                                                       .AsNoTracking()
                                                       .Where(x => x.TournamentId == Tournament.Id)
                                                       .Select(x => new VenueListWithBranch
                                                       {
                                                           VenueId = (long)x.VenueId,
                                                           VenueName = x.Venue.Name,
                                                           BranchName = x.Venue.Branch.Name,
                                                           BranchId = x.Venue.Branch.Id
                                                       })
                                                       .ToList(),
                                           //  BranchName = VenueList?.BranchName,
                                           StatKeeperList = _context.TournamentStatkeeper
                                                       .AsNoTracking()
                                                       .Where(x => x.TournamentId == Tournament.Id)
                                                       .Select(x => new GenericObj { Id = (long)x.StatkeeperId, Name = x.UserInfo.Name })
                                                       .ToList(),
                                           FirstPrize = (long)Tournament.FirstPrize,
                                           SecondPrize = (long)Tournament.SecondPrize,
                                           ThirdPrize = (long)Tournament.ThirdPrize,
                                           SeasonTeamCapacity=Tournament.SeasonTeamCapacity,
                                           SeasonEndDate=Tournament.SeasonEndDate,
                                           SeasonStartDate=Tournament.SeasonStartDate
                                       }).FirstOrDefaultAsync();

            return getTournament;
        }

        public async Task<List<GetAllTournament>> GetAllTournamentAsyncByBusiness(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId)
        {
            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking().Include(x => x.Activity).ThenInclude(x => x.ActivityCategory)
                              .Select(x => new
                              {
                                  x.StartDateTime,
                                  x.EndDateTime,
                                  x.Id,
                                  x.TeamCapacity,
                                  x.CreatedBy,
                                  x.Name,
                                  x.IsActive,
                                  x.IsDeleted,
                                  x.ActivityId,
                                  x.Activity,
                                  x.Activity.ActivityCategory
                              })

                               join tournamentVenueTbl in _context.TournamentVenue.AsNoTracking().Include(x => x.Venue.Branch).Where(x => x.Venue.Branch.UserInfoId == userId)  // Where Condition Added
                               .Select(x => new { x.Id, x.TournamentId, x.VenueId, x.Venue, x.Venue.Branch, x.IsActive, x.IsDeleted })
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               join tournamentTeamTbl in _context.TournamentTeam.AsNoTracking()
                               on tournamentTbl.Id equals tournamentTeamTbl.TournamentId into newTournamentTeam
                               from TournamentTeamFull in newTournamentTeam.DefaultIfEmpty()

                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on TournamentTeamFull.TeamId equals teamMemberTbl.TeamId into newTeamMember
                               from teamMemberFullTbl in newTeamMember.DefaultIfEmpty()

                               join userInfoTbl in _context.UserInfo.AsNoTracking().Include(x => x.User)
                               on tournamentTbl.CreatedBy equals userInfoTbl.User.Email

                               join image in _context.Image.Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                               on userInfoTbl.Id equals image.TableId into imageFull
                               from imageOuterLeft in imageFull.DefaultIfEmpty()

                               where imageOuterLeft != null ?
                               imageOuterLeft.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               imageOuterLeft.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                               : tournamentTbl.Id != 0

                               where venueId != 0 ? tournamentVenueTbl.VenueId == venueId : tournamentTbl.Id != 0


                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   TeamCapacity = tournamentTbl.TeamCapacity,
                                   BranchName = tournamentVenueTbl.Venue.Branch.Name,
                                   Name = tournamentTbl.Name,
                                   StartDateTime = tournamentTbl.StartDateTime,
                                   EndDateTime = tournamentTbl.EndDateTime,
                                   CreatedBy = userInfoTbl.Name,
                                   ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
                                   CreatedByImage = imageOuterLeft != null ? imageOuterLeft.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == tournamentTbl.ActivityId && x.ConnectionEntity.Name.ToLower()
                                                   == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                   TeamPlayedInTournament = _context.TournamentTeam.AsNoTracking().Where(t => t.TournamentId == tournamentTbl.Id && t.TeamId == teamId).Select(x => x.TeamId).FirstOrDefault(),
                                   JoinedStatus = TournamentTeamFull != null ? TournamentTeamFull.IsAccepted != false ? "Joined" : "Requested To join" : "Request to join"
                               }).Distinct().Where(s => tournamentStatus != TournamentStatus.All ?
                                           tournamentStatus == TournamentStatus.Upcoming ?
                                           s.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                                           tournamentStatus == TournamentStatus.Completed ?
                                           s.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                                           s.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                                           s.Id != 0)
                              .Where(t => teamId != 0 ? t.TeamPlayedInTournament == teamId : t.Id != 0)
                              .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                               .Take(paginationFilter.PageSize).ToListAsync();

            return query;
        }

        public async Task<int> GetAllTournamentCountAsyncByBusiness(long userId, TournamentStatus tournamentStatus, long teamId, long venueId)
        {
            var count = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                          .Select(x => new { x.StartDateTime, x.EndDateTime, x.Id, x.TeamCapacity, x.CreatedBy, x.Name, x.IsActive, x.IsDeleted })
                               join tournamentVenueTbl in _context.TournamentVenue.Include(x => x.Venue.Branch).Where(x => x.Venue.Branch.UserInfoId == userId)  // Where Condition Added
                               .Select(x => new { x.Id, x.TournamentId, x.VenueId, x.Venue, x.Venue.Branch, x.IsActive, x.IsDeleted })
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               where venueId != 0 ? tournamentVenueTbl.VenueId == venueId : tournamentTbl.Id != 0

                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                   TeamPlayedInTournament = _context.TournamentTeam.AsNoTracking().Where(t => t.TournamentId == tournamentTbl.Id && t.TeamId == teamId).Select(x => x.TeamId).FirstOrDefault(),
                               }).Where(s => tournamentStatus != TournamentStatus.All ?
                                                tournamentStatus == TournamentStatus.Upcoming ?
                                                s.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                                                tournamentStatus == TournamentStatus.Completed ?
                                                s.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                                                s.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                                                s.Id != 0)
                               .Where(t => teamId != 0 ? t.TeamPlayedInTournament == teamId : t.Id != 0)
                          .CountAsync();
            return count;
        }

        // Ammar's GetAllTournamentAsyncByPlayer
        //public async Task<List<GetAllTournament>> GetAllTournamentAsyncByPlayer(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId,
        //    IQueryable<Domain.Entities.Tournament> tournamentTbl, IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList, IQueryable<Images> imageActivityList,
        //     IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList, string sSearch)
        //    {
        //    List<GetAllTournament> getAllTournaments = new List<GetAllTournament>();


        //    if (venueId != 0)
        //    {
        //        tournamentVenueList = tournamentVenueList.Where(x => x.VenueId == venueId).AsQueryable();

        //        tournamentTbl = tournamentTbl.Join(tournamentVenueList,
        //                                           tournamentId => tournamentId.Id, tournamentVenue => tournamentVenue.TournamentId, (tournamentId, tournamentVenue) => tournamentId).AsQueryable();
        //    }
        //    if (teamId != 0)
        //    {
        //        tournamentTeamList = tournamentTeamList.Where(x => x.IsAccepted != false && x.TeamId == teamId).AsQueryable();
        //        tournamentTbl = tournamentTbl.Join(tournamentTeamList,
        //                                           tournamentId => tournamentId.Id, tournamentTeam => tournamentTeam.TournamentId, (tournamentId, tournamentTeam) => tournamentId).AsQueryable();
        //    }

        //    var joinedTeamStatus = tournamentTeamList.Where(x => teamIdList.Contains(x.TeamId)).AsQueryable();
        //    IQueryable<long> tournamentIdListInWhichUserTeamPlayed = joinedTeamStatus.Select(x => x.TournamentId).AsQueryable();

        //    getAllTournaments = await tournamentTbl.OrderByField(paginationFilter.SortBy, paginationFilter.IsAsc).Select(tournamentTbl => new GetAllTournament
        //    {
        //        Id = tournamentTbl.Id,
        //        Name = tournamentTbl.Name,
        //        BranchName = tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
        //                             tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).Select(x => x.Venue.Branch.Name).FirstOrDefault() : "No Branch Found",
        //        StartDateTime = tournamentTbl.StartDateTime,
        //        EndDateTime = tournamentTbl.EndDateTime,
        //        TeamCapacity = tournamentTbl.TeamCapacity,
        //        CreatedBy = userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Name).FirstOrDefault(),
        //        CreatedByImage = imageCreatedByImageList.Where(x => x.TableId == userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Id).FirstOrDefault()).Select(x => x.Path).FirstOrDefault(),
        //        ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
        //        ActivityIcon = imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId) != null ?
        //                                imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId).FirstOrDefault().Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
        //        Status = (DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) > 0 ?
        //                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) > 0 ?
        //                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
        //                                   DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) < 0 ?
        //                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
        //        JoinedStatus = joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
        //                               joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id && x.IsAccepted != false).FirstOrDefault() != null ? Constant.Joined
        //                               : Constant.REQUEST_PENDING
        //                               : Constant.REQUEST_TO_JOIN
        //    }).Where(tournament => tournamentStatus != TournamentStatus.All ?
        //                    tournamentStatus == TournamentStatus.Upcoming ?
        //                    tournament.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
        //                    tournamentStatus == TournamentStatus.Completed ?
        //                    tournament.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
        //                    tournament.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
        //                    tournament.Id != 0)
        //    .Where(x => !joinedTeamStatus.Any() ? x.Status.ToLower() == Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() : x.Id != 0)
        //    .Where(x => tournamentIdListInWhichUserTeamPlayed.Any() && x.Status.ToLower() != Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() ? tournamentIdListInWhichUserTeamPlayed.Contains(x.Id) : x.Id != 0)
        //    //.Where(x => x.BranchName.Contains(sSearch))


        //                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
        //                .Take(paginationFilter.PageSize).ToListAsync();

        //    return getAllTournaments;
        //}


        //public async Task<List<GetAllTournament>> GetAllTournamentAsyncByPlayer(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId,
        //    IQueryable<Domain.Entities.Tournament> tournamentTbl, IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList, IQueryable<Images> imageActivityList,
        //     IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList, string sSearch)
        //{
        //        List<GetAllTournament> getAllTournaments = new List<GetAllTournament>();


        //    if (venueId != 0)
        //    {
        //        tournamentVenueList = tournamentVenueList.Where(x => x.VenueId == venueId).AsQueryable();

        //        tournamentTbl = tournamentTbl.Join(tournamentVenueList,
        //                                           tournamentId => tournamentId.Id, tournamentVenue => tournamentVenue.TournamentId, (tournamentId, tournamentVenue) => tournamentId).AsQueryable();
        //    }
        //    if (teamId != 0)
        //    {
        //        tournamentTeamList = tournamentTeamList.Where(x => x.IsAccepted != false && x.TeamId == teamId).AsQueryable();
        //        tournamentTbl = tournamentTbl.Join(tournamentTeamList,
        //                                           tournamentId => tournamentId.Id, tournamentTeam => tournamentTeam.TournamentId, (tournamentId, tournamentTeam) => tournamentId).AsQueryable();
        //    }

        //    var joinedTeamStatus = tournamentTeamList.Where(x => teamIdList.Contains(x.TeamId)).AsQueryable();
        //    IQueryable<long> tournamentIdListInWhichUserTeamPlayed = joinedTeamStatus.Select(x => x.TournamentId).AsQueryable();

        //    getAllTournaments = await tournamentTbl.OrderByField(paginationFilter.SortBy, paginationFilter.IsAsc).Select(tournamentTbl => new GetAllTournament
        //    {
        //        Id = tournamentTbl.Id,
        //        Name = tournamentTbl.Name,
        //        BranchName = tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
        //                             tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).Select(x => x.Venue.Branch.Name).FirstOrDefault() : "No Branch Found",
        //        StartDateTime = tournamentTbl.StartDateTime,
        //        EndDateTime = tournamentTbl.EndDateTime,
        //        TeamCapacity = tournamentTbl.TeamCapacity,
        //        CreatedBy = userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Name).FirstOrDefault(),
        //        CreatedByImage = imageCreatedByImageList.Where(x => x.TableId == userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Id).FirstOrDefault()).Select(x => x.Path).FirstOrDefault(),
        //        ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
        //        ActivityIcon = imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId) != null ?
        //                                imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId).FirstOrDefault().Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,

        //        Status = (DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) > 0 ?
        //                                            Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) > 0 ?
        //                                            Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
        //                                            DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) < 0 ?
        //                                            Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
        //        JoinedStatus = joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
        //                                                        joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id && x.IsAccepted != false).FirstOrDefault() != null ? Constant.Joined
        //                                                        : Constant.REQUEST_PENDING
        //                                                        : Constant.REQUEST_TO_JOIN
        //    })
        //                    .Where(tournament => tournamentStatus != TournamentStatus.All ?
        //                    tournamentStatus == TournamentStatus.Upcoming ?
        //                    tournament.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
        //                    tournamentStatus == TournamentStatus.Completed ?
        //                    tournament.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
        //                    tournament.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
        //                    tournament.Id != 0)
        //                            .Where(x => !joinedTeamStatus.Any() ? x.Status.ToLower() == Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() : x.Id != 0)
        //                            .Where(x => tournamentIdListInWhichUserTeamPlayed.Any() && x.Status.ToLower() != Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() ? tournamentIdListInWhichUserTeamPlayed.Contains(x.Id) : x.Id != 0)
        //                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
        //                .Take(paginationFilter.PageSize).ToListAsync();


        //    return getAllTournaments;
        //}

        //Copied
        public async Task<List<GetAllTournament>> GetAllTournamentAsyncByPlayer(long userId, BasicFilter paginationFilter,
            TournamentStatus tournamentStatus, long teamId, long venueId, IQueryable<Domain.Entities.Tournament> tournamentTbl,
            IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList, IQueryable<Images> imageActivityList,
            IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList)
        {
            List<GetAllTournament> getAllTournaments = new List<GetAllTournament>();
            List<GetAllTournament> getAllTournamentsTemp = new List<GetAllTournament>();

            if (venueId != 0)
            {
                tournamentVenueList = tournamentVenueList.Where(x => x.VenueId == venueId).AsQueryable();

                tournamentTbl = tournamentTbl.Join(tournamentVenueList,
                                                   tournamentId => tournamentId.Id, tournamentVenue => tournamentVenue.TournamentId, (tournamentId, tournamentVenue) => tournamentId).AsQueryable();
            }
            if (teamId != 0)
            {
                tournamentTeamList = tournamentTeamList.Where(x => x.IsAccepted != false && x.TeamId == teamId).AsQueryable();
                tournamentTbl = tournamentTbl.Join(tournamentTeamList,
                                                   tournamentId => tournamentId.Id, tournamentTeam => tournamentTeam.TournamentId, (tournamentId, tournamentTeam) => tournamentId).AsQueryable();
            }

            var joinedTeamStatus = tournamentTeamList.Where(x => teamIdList.Contains(x.TeamId)).AsQueryable();
            IQueryable<long> tournamentIdListInWhichUserTeamPlayed = joinedTeamStatus.Select(x => x.TournamentId).AsQueryable();


            getAllTournaments = await tournamentTbl.OrderByField(paginationFilter.SortBy, paginationFilter.IsAsc).Select(tournamentTbl => new GetAllTournament
            {
                Id = tournamentTbl.Id,
                Name = tournamentTbl.Name,
                BranchName = tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
                                     tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).Select(x => x.Venue.Branch.Name).FirstOrDefault() : "No Branch Found",
                StartDateTime = tournamentTbl.StartDateTime,
                EndDateTime = tournamentTbl.EndDateTime,
                TeamCapacity = tournamentTbl.TeamCapacity,
                CreatedBy = userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Name).FirstOrDefault(),
                CreatedByImage = imageCreatedByImageList.Where(x => x.TableId == userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Id).FirstOrDefault()).Select(x => x.Path).FirstOrDefault(),
                ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
                ActivityIcon = imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId) != null ?
                                imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId).FirstOrDefault().Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                Status = (DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) > 0 ?
                                           Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) > 0 ?
                                           Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
                                           DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) < 0 ?
                                           Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                JoinedStatus = joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
                                       joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id && x.IsAccepted != false).FirstOrDefault() != null ? Constant.Joined
                                       : Constant.REQUEST_PENDING
                                       : Constant.REQUEST_TO_JOIN
            }).Where(tournament => tournamentStatus != TournamentStatus.All ?
                         tournamentStatus == TournamentStatus.Upcoming ?
                         tournament.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                         tournamentStatus == TournamentStatus.Completed ?
                         tournament.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                         tournament.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                         tournament.Id != 0)
            .Where(x => !joinedTeamStatus.Any() ? x.Status.ToLower() == Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() : x.Id != 0)
            .Where(x => tournamentIdListInWhichUserTeamPlayed.Any() && x.Status.ToLower() != Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() ? tournamentIdListInWhichUserTeamPlayed.Contains(x.Id) : x.Id != 0)
            .Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?
                                          x.Status.ToLower().Trim().Contains(paginationFilter.sSearch.ToLower().Trim()) :
                                          x.Id != 0)
                        .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                        .Take(paginationFilter.PageSize).ToListAsync();


            return getAllTournaments;
        }

        public async Task<int> GetAllTournamentCountAsyncByPlayer(long userId, TournamentStatus tournamentStatus, long teamId, long venueId,
            IQueryable<Domain.Entities.Tournament> tournamentTbl, IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList, IQueryable<Images> imageActivityList,
             IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList, string sSearch)
        {
            int getAllTournaments = 0;

            if (venueId != 0)
            {
                tournamentVenueList = tournamentVenueList.Where(x => x.VenueId == venueId).AsQueryable();

                tournamentTbl = tournamentTbl.Join(tournamentVenueList,
                                                   tournamentId => tournamentId.Id, tournamentVenue => tournamentVenue.TournamentId, (tournamentId, tournamentVenue) => tournamentId).AsQueryable();
            }
            if (teamId != 0)
            {
                tournamentTeamList = tournamentTeamList.Where(x => x.IsAccepted != false && x.TeamId == teamId).AsQueryable();
                tournamentTbl = tournamentTbl.Join(tournamentTeamList,
                                                   tournamentId => tournamentId.Id, tournamentTeam => tournamentTeam.TournamentId, (tournamentId, tournamentTeam) => tournamentId).AsQueryable();
            }

            var joinedTeamStatus = tournamentTeamList.Where(x => teamIdList.Contains(x.TeamId)).AsQueryable();
            IQueryable<long> tournamentIdListInWhichUserTeamPlayed = joinedTeamStatus.Select(x => x.TournamentId).AsQueryable();

            getAllTournaments = await tournamentTbl.Select(tournamentTbl => new GetAllTournament
            {
                Id = tournamentTbl.Id,
                Name = tournamentTbl.Name,
                BranchName = tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
                                      tournamentVenueList.Where(x => x.TournamentId == tournamentTbl.Id).Select(x => x.Venue.Branch.Name).FirstOrDefault() : "No Branch Found",
                StartDateTime = tournamentTbl.StartDateTime,
                EndDateTime = tournamentTbl.EndDateTime,
                TeamCapacity = tournamentTbl.TeamCapacity,
                CreatedBy = userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Name).FirstOrDefault(),
                CreatedByImage = imageCreatedByImageList.Where(x => x.TableId == userInfo.Where(x => x.User.Email == tournamentTbl.CreatedBy).Select(x => x.Id).FirstOrDefault()).Select(x => x.Path).FirstOrDefault(),
                ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
                ActivityIcon = imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId) != null ?
                                         imageActivityList.Where(x => x.TableId == tournamentTbl.ActivityId).FirstOrDefault().Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                Status = (DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 || DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0) && DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) > 0 ?
                                            Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) > 0 ?
                                            Domain.Enums.Enum.TournamentStatus.Upcoming.ToString() :
                                            DateTime.Compare(tournamentTbl.EndDateTime, DateTime.UtcNow) < 0 ?
                                            Domain.Enums.Enum.TournamentStatus.Completed.ToString() : Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                JoinedStatus = joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id).FirstOrDefault() != null ?
                                        joinedTeamStatus.Where(x => x.TournamentId == tournamentTbl.Id && x.IsAccepted != false).FirstOrDefault() != null ? Constant.Joined
                                        : Constant.REQUEST_PENDING
                                        : Constant.REQUEST_TO_JOIN
            }).Where(tournament => tournamentStatus != TournamentStatus.All ?
                            tournamentStatus == TournamentStatus.Upcoming ?
                            tournament.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                            tournamentStatus == TournamentStatus.Completed ?
                            tournament.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                            tournament.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                            tournament.Id != 0)
             .Where(x => !joinedTeamStatus.Any() ? x.Status.ToLower() == Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() : x.Id != 0)
             .Where(x => tournamentIdListInWhichUserTeamPlayed.Any() && x.Status.ToLower() != Domain.Enums.Enum.TournamentStatus.Upcoming.ToString().ToLower() ? tournamentIdListInWhichUserTeamPlayed.Contains(x.Id) : x.Id != 0)
             .Where(x => !string.IsNullOrEmpty(sSearch) ?
                                          x.Status.ToLower().Trim().Contains(sSearch.ToLower().Trim()) :
                                          x.Id != 0)
                        .CountAsync();
            return getAllTournaments;

        }

        public async Task<List<GetAllTournament>> GetAllTournamentAsyncByStatkeeper(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId)
        {
            /*var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                               .Select(x => new { x.StartDateTime, x.EndDateTime, x.Id, x.TeamCapacity, x.CreatedBy, x.Name, x.IsActive, x.IsDeleted, x.ActivityId })
                               join tournamentVenueTbl in _context.TournamentVenue.AsNoTracking().Include(x => x.Venue.Branch)
                               .Select(x => new { x.Id, x.TournamentId, x.VenueId, x.Venue, x.Venue.Branch, x.IsActive, x.IsDeleted })
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               join userInfoTbl in _context.UserInfo.AsNoTracking().Include(x => x.User)
                               on tournamentTbl.CreatedBy equals userInfoTbl.User.Email

                               join tounrnamentTeamTbl in _context.TournamentTeam
                               on tournamentTbl.Id equals tounrnamentTeamTbl.TournamentId

                               join team in _context.Team
                               on tounrnamentTeamTbl.TeamId equals team.Id

                               join tournamentStatkeeper in _context.TournamentStatkeeper
                               on tournamentTbl.Id equals tournamentStatkeeper.TournamentId

                               where tournamentStatkeeper.StatkeeperId == userId

                               join teamMemeber in _context.TeamMember.Include(x => x.UserInfo).Where(x => x.UserInfo.Id == userId)
                               on team.Id equals teamMemeber.TeamId

                               join image in _context.Image.Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                               on userInfoTbl.Id equals image.TableId into imageFull
                               from imageOuterLeft in imageFull.DefaultIfEmpty()

                               where imageOuterLeft != null ?
                               imageOuterLeft.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               imageOuterLeft.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                               : tournamentTbl.Id != 0


                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   TeamCapacity = tournamentTbl.TeamCapacity,
                                   BranchName = tournamentVenueTbl.Venue.Branch.Name,
                                   Name = tournamentTbl.Name,
                                   StartDateTime = tournamentTbl.StartDateTime,
                                   EndDateTime = tournamentTbl.EndDateTime,
                                   CreatedBy = userInfoTbl.Name,
                                   CreatedByImage = imageOuterLeft != null ? imageOuterLeft.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == tournamentTbl.ActivityId && x.ConnectionEntity.Name.ToLower()
                                                   == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                   TeamPlayedInTournament = _context.TournamentTeam.AsNoTracking().Where(t => t.TournamentId == tournamentTbl.Id && t.TeamId == teamId).Select(x => x.TeamId).FirstOrDefault(),

                               }).Where(s => tournamentStatus != TournamentStatus.All ?
                                           tournamentStatus == TournamentStatus.Upcoming ?
                                           s.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                                           tournamentStatus == TournamentStatus.Completed ?
                                           s.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                                           s.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                                           s.Id != 0)
                               .Where(t => teamId != 0 ? t.TeamPlayedInTournament == teamId : t.Id != 0)
                               .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                    .Take(paginationFilter.PageSize).Distinct().ToListAsync();    // Add Distinct here 
            return query;*/

            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                               .Select(x => new { x.StartDateTime, x.EndDateTime, x.Id, x.TeamCapacity, x.CreatedBy, x.Name, x.IsActive, x.IsDeleted, x.ActivityId })
                               join tournamentVenueTbl in _context.TournamentVenue.AsNoTracking().Include(x => x.Venue.Branch)
                               .Select(x => new { x.Id, x.TournamentId, x.VenueId, x.Venue, x.Venue.Branch, x.IsActive, x.IsDeleted })
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               join userInfoTbl in _context.UserInfo.AsNoTracking().Include(x => x.User)
                               on tournamentTbl.CreatedBy equals userInfoTbl.User.Email

                               join tounrnamentTeamTbl in _context.TournamentTeam
                               on tournamentTbl.Id equals tounrnamentTeamTbl.TournamentId

                               join tournamentStatkeeper in _context.TournamentStatkeeper.AsNoTracking().Where(x => x.StatkeeperId == userId)
                               on tournamentTbl.Id equals tournamentStatkeeper.TournamentId

                               join team in _context.Team
                               on tounrnamentTeamTbl.TeamId equals team.Id

                               join teamMemeber in _context.TeamMember.Include(x => x.UserInfo)
                               on team.Id equals teamMemeber.TeamId

                               join image in _context.Image.Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                               on userInfoTbl.Id equals image.TableId into imageFull
                               from imageOuterLeft in imageFull.DefaultIfEmpty()

                               where imageOuterLeft != null ?
                               imageOuterLeft.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               imageOuterLeft.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                               : tournamentTbl.Id != 0


                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   TeamCapacity = tournamentTbl.TeamCapacity,
                                   BranchName = tournamentVenueTbl.Venue.Branch.Name,
                                   Name = tournamentTbl.Name,
                                   StartDateTime = tournamentTbl.StartDateTime,
                                   EndDateTime = tournamentTbl.EndDateTime,
                                   CreatedBy = userInfoTbl.Name,
                                   CreatedByImage = imageOuterLeft != null ? imageOuterLeft.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == tournamentTbl.ActivityId && x.ConnectionEntity.Name.ToLower()
                                                   == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                   TeamPlayedInTournament = _context.TournamentTeam.AsNoTracking().Where(t => t.TournamentId == tournamentTbl.Id && t.TeamId == teamId).Select(x => x.TeamId).FirstOrDefault(),

                               }).Where(s => tournamentStatus != TournamentStatus.All ?
                                           tournamentStatus == TournamentStatus.Upcoming ?
                                           s.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                                           tournamentStatus == TournamentStatus.Completed ?
                                           s.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                                           s.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                                           s.Id != 0)
                                .Where(t => teamId != 0 ? t.TeamPlayedInTournament == teamId : t.Id != 0)
                                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                .Take(paginationFilter.PageSize)
                                .Distinct().ToListAsync();  // Add Distinct here 
            return query;
        }

        public async Task<int> GetAllTournamentCountAsyncByStatkeeper(long userId, TournamentStatus tournamentStatus, long teamId, long venueId)
        {
            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                    .Select(x => new { x.StartDateTime, x.EndDateTime, x.Id, x.TeamCapacity, x.CreatedBy, x.Name, x.IsActive, x.IsDeleted, x.ActivityId })
                               join tournamentVenueTbl in _context.TournamentVenue.AsNoTracking().Include(x => x.Venue.Branch)
                               .Select(x => new { x.Id, x.TournamentId, x.VenueId, x.Venue, x.Venue.Branch, x.IsActive, x.IsDeleted })
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               join userInfoTbl in _context.UserInfo.AsNoTracking().Include(x => x.User)
                               on tournamentTbl.CreatedBy equals userInfoTbl.User.Email

                               join tounrnamentTeamTbl in _context.TournamentTeam
                               on tournamentTbl.Id equals tounrnamentTeamTbl.TournamentId

                               join tournamentStatkeeper in _context.TournamentStatkeeper.AsNoTracking().Where(x => x.StatkeeperId == userId)

                               on tournamentTbl.Id equals tournamentStatkeeper.TournamentId

                               join team in _context.Team
                               on tounrnamentTeamTbl.TeamId equals team.Id

                               join teamMemeber in _context.TeamMember.Include(x => x.UserInfo)
                               on team.Id equals teamMemeber.TeamId

                               join image in _context.Image.Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                               on userInfoTbl.Id equals image.TableId into imageFull
                               from imageOuterLeft in imageFull.DefaultIfEmpty()

                               where imageOuterLeft != null ?
                               imageOuterLeft.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               imageOuterLeft.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                               : tournamentTbl.Id != 0


                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   TeamCapacity = tournamentTbl.TeamCapacity,
                                   BranchName = tournamentVenueTbl.Venue.Branch.Name,
                                   Name = tournamentTbl.Name,
                                   StartDateTime = tournamentTbl.StartDateTime,
                                   EndDateTime = tournamentTbl.EndDateTime,
                                   CreatedBy = userInfoTbl.Name,
                                   CreatedByImage = imageOuterLeft != null ? imageOuterLeft.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == tournamentTbl.ActivityId && x.ConnectionEntity.Name.ToLower()
                                                   == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                                   TeamPlayedInTournament = _context.TournamentTeam.AsNoTracking().Where(t => t.TournamentId == tournamentTbl.Id && t.TeamId == teamId).Select(x => x.TeamId).FirstOrDefault(),

                               }).Where(s => tournamentStatus != TournamentStatus.All ?
                                           tournamentStatus == TournamentStatus.Upcoming ?
                                           s.Status.ToLower() == TournamentStatus.Upcoming.ToString().ToLower() :
                                           tournamentStatus == TournamentStatus.Completed ?
                                           s.Status.ToLower() == TournamentStatus.Completed.ToString().ToLower() :
                                           s.Status.ToLower() == TournamentStatus.Active.ToString().ToLower() :
                                           s.Id != 0)
                                .Where(t => teamId != 0 ? t.TeamPlayedInTournament == teamId : t.Id != 0)
                                .Distinct().CountAsync();    // Add Distinct here 
            return query;
        }

        public async Task<List<GenericObj>> GetStatkeeperListByTournamentIdAsync(long tournamentId)
        {
            var query = await (from tv in _context.TournamentVenue
                               join vk in _context.VenueStatkeeper
                               on tv.VenueId equals vk.VenueId
                               join tk in _context.TournamentStatkeeper
                               on vk.UserInfoId equals tk.StatkeeperId into temp
                               from t in temp.DefaultIfEmpty()
                               join ui in _context.UserInfo on vk.UserInfoId equals ui.Id
                               where t == null && tv.TournamentId == tournamentId
                               select new GenericObj
                               {
                                   Id = ui.Id,
                                   Name = ui.Name
                               }).ToListAsync();

            return query;
        }

        public async Task<List<TeamGenericObj>> GetTournamentTeamsListAsync(long tournamentId, long userId)
        {
            var query = await (from tournament in _context.Tournament.AsNoTracking()
                               join tournamentTeam in _context.TournamentTeam.AsNoTracking()
                               on tournament.Id equals tournamentTeam.TournamentId

                               join team in _context.Team.AsNoTracking()
                               on tournamentTeam.TeamId equals team.Id
                               where tournamentTeam.TournamentId == tournamentId && tournamentTeam.IsAccepted != false

                               select new TeamGenericObj
                               {
                                   Id = team.Id,
                                   Name = team.Name,
                                   Colour = team.Colour,
                               }).ToListAsync();
            return query;
        }

        public async Task<List<RequestedTeamsInTournament>> GetTournamentTeamsRequestAsync(BasicFilter paginationFilter, long tournamentId)
        {
            var query = await _context.TournamentTeam.AsNoTracking()
                .Where(x => x.TournamentId == tournamentId && !x.IsAccepted && x.IsActive && !x.IsDeleted)
                .Include(x => x.Team)
                .Select(x => new RequestedTeamsInTournament { Id = x.TeamId, Name = x.Team.Name, Color = x.Team.Colour })
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                     .Take(paginationFilter.PageSize)
                     .ToListAsync();

            return query;
        }

        public async Task<int> GetTournamentTeamsRequestAsyncCount(long tournamentId)
        {
            var query = await _context.TournamentTeam.AsNoTracking()
              .Where(x => x.TournamentId == tournamentId && !x.IsAccepted && x.IsActive && !x.IsDeleted)
              .Include(x => x.Team)
              .Select(x => new RequestedTeamsInTournament { Id = x.TeamId, Name = x.Team.Name, Color = x.Team.Colour }).CountAsync();
            return query;
        }

        public async Task<List<TournamentPlayoffResponse>> GetTournamentPlayoffByIdAsync(long tournamentId)
        {
            var query = await (from eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking().Include(x => x.Team)
                               on eventCollectionTbl.Id equals eventCollectionTeamTbl.EventCollectionId

                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId

                               join tournamentEventCollection in _context.TournamentEventCollection.AsNoTracking()
                               on eventCollectionTbl.Id equals tournamentEventCollection.EventCollectionId

                               where tournamentEventCollection.TournamentId == tournamentId && eventTeamTbl.Round != -1
                               group eventTeamTbl by eventTeamTbl.EventId into groupEventTeamTbl

                               select new TournamentPlayoffResponse
                               {
                                   EventId = groupEventTeamTbl.Key,
                                   Group = groupEventTeamTbl.First().Group.ToString(),
                                   TeamDetailList = groupEventTeamTbl.Select(t => new TeamDetail
                                   {
                                       Name = t.EventCollectionTeam.Team.Name,
                                       Id = t.EventCollectionTeam.Team.Id,
                                       Round = t.Round,
                                       Points = t.Points,
                                       Status = t.EventStatus.Name
                                   }).ToList()
                               }).ToListAsync();

            return query;
        }

        public async Task<List<Application.Response.Tournament.Event>> GetTournamentEventsByIdAsync(long tournamentId)
        {
            var query = await (from eventTbl in _context.Event.Select(e => new { e.Id, e.Name, e.EventDateTime, e.IsActive, e.IsDeleted })
                               join eventTeamTbl in _context.EventTeam
                                .Include(x => x.EventCollectionTeam)
                                .Include(x => x.EventCollectionTeam.EventCollection)
                                .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                                .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                                .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch)
                                .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                                .Include(x => x.EventStatus)
                                .Include(x => x.Event.UserInfo.User)
                               on eventTbl.Id equals eventTeamTbl.EventId
                               join eventCollectionTeamTbl in _context.EventCollectionTeam
                               on eventTeamTbl.EventCollectionTeamId equals eventCollectionTeamTbl.Id
                               join eventCollectionTbl in _context.EventCollection
                               on eventCollectionTeamTbl.EventCollectionId equals eventCollectionTbl.Id
                               join tournamentEventCollectionTbl in _context.TournamentEventCollection.Include(t => t.Tournament)
                               on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId
                               where tournamentEventCollectionTbl.TournamentId == tournamentId
                               group eventTeamTbl by eventTeamTbl.EventId into eventTeamGroup

                               select new Application.Response.Tournament.Event
                               {
                                   EventId = eventTeamGroup.Key,
                                   EventName = eventTeamGroup.First<EventTeam>().Event.Name,
                                   EventCollectionId = eventTeamGroup.First<EventTeam>().EventCollectionTeam.EventCollection.Id,
                                   VenueId = eventTeamGroup.First<EventTeam>().EventCollectionTeam.EventCollection.Venue.Id,
                                   VenueName = eventTeamGroup.First<EventTeam>().EventCollectionTeam.EventCollection.Venue.Name,
                                   EventDateTime = eventTeamGroup.First<EventTeam>().Event.EventDateTime.ToShortDateString(),
                                   TeamList = eventTeamGroup.Select<EventTeam, TeamDetailWithStatus>(t => new TeamDetailWithStatus
                                   {
                                       Name = t.EventCollectionTeam.Team.Name,
                                       Id = t.EventCollectionTeam.Team.Id,
                                       Status = t.EventStatus.Name
                                   }).ToList()
                               }).ToListAsync<Application.Response.Tournament.Event>();

            return query;
        }

        public async Task<TournamentTeam> GetTournamentTeamByIdsAsync(long tournamentId, long teamId)
        {
            return await _context.TournamentTeam.AsNoTracking().Where(x => x.TournamentId == tournamentId && x.TeamId == teamId).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckTournamentActivityMatchesTeamActivityAsync(long tournamentId, long teamId)
        {
            var query = await (from team in _context.Team.Where(x => x.Id == teamId)
                               join teamactivity in _context.TeamActivity
                               on team.Id equals teamactivity.TeamId
                               join tournament in _context.Tournament.Where(x => x.Id == tournamentId)
                               on teamactivity.ActivityId equals tournament.ActivityId
                               select tournament).FirstOrDefaultAsync();
            return query is not null ? true : false;
        }


        public async Task<List<TournamentPrizeRes>> TournamentPrizeQuery(long tournamentId)
        {
            var results = await _context.Tournament.AsNoTracking().Where(x => x.Id == tournamentId).Select(t =>
                                 new TournamentPrizeRes
                                 {
                                     FirstPrize = "$" + t.FirstPrize.ToString(),
                                     SecondPrize = "$" + t.SecondPrize.ToString(),
                                     ThirdPrize = "$" + t.ThirdPrize.ToString()
                                 }).ToListAsync();

            return results;

        }

        public async Task<List<long>> GetTournamentTopTeamsToSchedulePlayoffRound(long tournamentId)
        {
            List<long> teamIdListForPlayOff = new List<long>();

            IQueryable<Domain.Entities.Tournament> tournamentList = _context.Tournament.AsNoTracking().AsQueryable();

            int topTeamCount = tournamentList.Where(x => x.Id == tournamentId).Select(x => x.TeamCapacity).FirstOrDefault();

            teamIdListForPlayOff = await (from tournamentTbl in tournamentList
                                          join tournamentEventCollection in _context.TournamentEventCollection.AsNoTracking()
                                          on tournamentTbl.Id equals tournamentEventCollection.TournamentId
                                          join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                                          on tournamentEventCollection.EventCollectionId equals eventCollectionTeamTbl.EventCollectionId
                                          join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                          on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId

                                          where tournamentTbl.Id == tournamentId && eventTeamTbl.Round == -1

                                          orderby eventTeamTbl.Points descending

                                          select eventCollectionTeamTbl.TeamId
                                          ).Distinct().Take(topTeamCount).ToListAsync();

            return teamIdListForPlayOff;
        }


        public List<TournamentStatkeeper> getTournamentstatkeeper(UpdateTournamentReq updateTournament)
        {
            return _context.TournamentStatkeeper.Where(x => x.TournamentId == updateTournament.Id).ToList();

        }
        public List<TournamentVenue> getTournamentVenue(UpdateTournamentReq updateTournament)
        {
            return _context.TournamentVenue.Where(x => x.TournamentId == updateTournament.Id).ToList();
        }
        public long getActivityId(long activityCatId)
        {
            var results = _context.Activity.Where(x => x.ActivityCategoryId == activityCatId &&
                                                  x.EventTypeId.ToString() != Domain.Enums.Enum.EventType.Pickup.ToString())
                                                  .Select(x=>x.Id)
                                                  .FirstOrDefault();
            return results;
        }
    }
}
