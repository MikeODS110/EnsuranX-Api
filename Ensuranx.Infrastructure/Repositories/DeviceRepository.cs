using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Device;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Ensuranx.Domain.Common.Errors.Errors;

namespace Ensuranx.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<long> GetEventCollection(string deviceToken, string email, long userId)
        {
            var device = await _context.Device.Where(x => x.DeviceToken == deviceToken).FirstOrDefaultAsync();
            var deviceId = device.Id;
            var eventCollection = await _context.EventCollection.Where(x => /*x.DeviceId == deviceId &&*/ x.IsDeleted!=true && x.IsActive!=false).FirstOrDefaultAsync();
            var results = eventCollection.Id;
            return results;
        }

        public async Task<Domain.Entities.Device> GetDeviceByDeviceId(string deviceId, long userId)
        {
            return await _context.Device.AsNoTracking().Where(x => x.DeviceToken == deviceId).FirstOrDefaultAsync();
        }

        public async Task<List<GetWaitingList>> GetWaitingList(long eventCollection)
        {
            List<GetWaitingList> waitingList = new List<GetWaitingList>();

            waitingList = await (from waitingTbl in _context.WaitingList.AsNoTracking().Include(x => x.UserInfo)
                                 join userInfo in _context.UserInfo.AsNoTracking()
                                 on waitingTbl.UserInfoId equals userInfo.Id

                                 where waitingTbl.EventCollectionId == eventCollection
                                 where waitingTbl.IsDeleted != true && waitingTbl.IsActive != false

                                 orderby waitingTbl.Id ascending

                                 select new GetWaitingList
                                 {
                                     Id = waitingTbl.UserInfoId,
                                     Name = waitingTbl.UserInfo.Name,
                                     Sequence = waitingTbl.PlayerNumber < 10 ? "0" + waitingTbl.PlayerNumber.ToString() : waitingTbl.PlayerNumber.ToString(),
                                 }).ToListAsync();

            return waitingList;
        }

        public async Task<List<GetWaitingList>> GetWaitingListByEventId(long eventId, long eventCollectionId)
        {
            return await (from eventTeamTbl in _context.EventTeam.AsNoTracking()
                          join teamMemberTbl in _context.TeamMember.AsNoTracking()
                          on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId

                          join waitingListTbl in _context.WaitingList.AsNoTracking()
                          on teamMemberTbl.PlayerId equals waitingListTbl.UserInfoId

                          where eventTeamTbl.EventId == eventId && waitingListTbl.EventCollectionId == eventCollectionId
                          && waitingListTbl.IsDeleted != true && waitingListTbl.IsActive != false

                          select new GetWaitingList
                          {
                              Id = waitingListTbl.UserInfoId,
                              Name = waitingListTbl.UserInfo.Name,
                              Sequence = waitingListTbl.PlayerNumber < 10 ? "0" + waitingListTbl.PlayerNumber.ToString() : waitingListTbl.PlayerNumber.ToString(),
                          }).ToListAsync();
        }

        public async Task<List<WaitingList>> GetWaitingListDeletePlayerList(long eventCollection)
        {
            List<WaitingList> waitingList = new List<WaitingList>();

            waitingList = await (from waitingTbl in _context.WaitingList.AsNoTracking().Include(x => x.UserInfo).Include(x => x.EventCollection)
                                 join userInfo in _context.UserInfo.AsNoTracking()
                                 on waitingTbl.UserInfoId equals userInfo.Id

                                 where waitingTbl.EventCollectionId == eventCollection
                                 //where waitingTbl.IsDeleted != false && waitingTbl.IsActive != true

                                 orderby waitingTbl.Id ascending

                                 select waitingTbl).ToListAsync();

            return waitingList;

        }

        public async Task<WaitingList> GetWaitingListByPlayerIdDeletedOne(long playerId, long eventCollectionId)
        {
            return await _context.WaitingList.FirstOrDefaultAsync(x => x.UserInfoId == playerId && x.IsDeleted != false && x.IsActive != true && x.EventCollectionId == eventCollectionId);
        }

        public async Task<WaitingList> GetWaitingListByPlayerId(long playerId, long eventCollectionId)
        {
            return await _context.WaitingList.FirstOrDefaultAsync(x => x.UserInfoId == playerId && x.EventCollectionId == eventCollectionId);
        }

        public async Task<WaitingList> GetWaitingListByPlayerId(long playerId)
        {
            return await _context.WaitingList.FirstOrDefaultAsync(x => x.UserInfoId == playerId);
        }

        public async Task<WaitingList> GetWaitingListByEventCollectionIdAndUserId(long eventCollection, long userId)
        {
            return await _context.WaitingList.AsNoTracking().Where(x => x.EventCollectionId == eventCollection && x.UserInfoId == userId).FirstOrDefaultAsync();
        }
        public async Task<bool> GetWaitingListByLobby(string deviceToken, long userInfoId)
        {
            var devices= await _context.Device.Where(x => x.DeviceToken == deviceToken).FirstOrDefaultAsync();
            var deviceId = devices.Id;

            var eventCollection = await _context.EventCollection/*.Where(x => x.DeviceId == deviceId)*/.FirstOrDefaultAsync();
            var eventCollectionId = eventCollection.Id;

            var results = await _context.WaitingList.AsNoTracking()
                                .AnyAsync(x => x.EventCollectionId == eventCollectionId && x.UserInfoId == userInfoId);

            return results;

        }

        public async Task<List<WaitingList>> ShiftThesePlayerToWaitingListAsync(long eventCollectionId, List<long> userInfoIdList)
        {
            List<WaitingList> playerToShiftList = await _context.WaitingList.Where(x => x.EventCollectionId == eventCollectionId && userInfoIdList.Contains(x.UserInfoId)).ToListAsync();
            return playerToShiftList;
        }

        public async Task<List<WaitingList>> UpdateWaitingListAsync(List<WaitingList> waitingList)
        {
            _context.WaitingList.UpdateRange(waitingList);
            await _context.SaveChangesAsync();

            return waitingList;
        }

        public async Task<List<WaitingList>> CreateWaitingListAsync(List<WaitingList> waitingList)
        {
            await _context.WaitingList.AddRangeAsync(waitingList);
            await _context.SaveChangesAsync();

            return waitingList;
        }

        public async Task<int> GetLastPlayerNumberInTheList(long eventCollection)
        {
            return await _context.WaitingList.AsNoTracking().Where(x => x.EventCollectionId == eventCollection).OrderByDescending(x => x.Id).Select(x => x.PlayerNumber).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckIfThisUserIsAPartOfAnyTeamInTheEventCollection(long eventCollection, long userId)
        {
            var query = await (from eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on eventCollectionTbl.Id equals eventCollectionTeamTbl.EventCollectionId
                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on eventCollectionTeamTbl.TeamId equals teamMemberTbl.TeamId

                               where eventCollectionTbl.Id == eventCollection && teamMemberTbl.PlayerId == userId && teamMemberTbl.IsJoinedCollection != false

                               select eventCollectionTbl).FirstOrDefaultAsync();

            return query is not null ? true : false;
        }
        
        public async Task<bool> CheckIfThisUserIsAPartOfAnyTeamInTheEventCollectionIsJoinedCollectionFalse(long eventCollection, long userId)
        {
            var query = await (from eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on eventCollectionTbl.Id equals eventCollectionTeamTbl.EventCollectionId
                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on eventCollectionTeamTbl.TeamId equals teamMemberTbl.TeamId

                               where eventCollectionTbl.Id == eventCollection && teamMemberTbl.PlayerId == userId && teamMemberTbl.IsJoinedCollection != true

                               select eventCollectionTbl).FirstOrDefaultAsync();

            return query is not null ? true : false;
        }

        public async Task<WaitingList> GetPlayerFromWaitingList(string deviceToken, long userInfoId)
        {
            WaitingList waitingList = await (from deviceTbl in _context.Device.AsNoTracking()
                              join eventCollectionTbl in _context.EventCollection.AsNoTracking()
                              on deviceTbl.Id equals eventCollectionTbl.ActivityId
                              join waitingListTbl in _context.WaitingList.AsNoTracking()
                              on eventCollectionTbl.Id equals waitingListTbl.EventCollectionId
                              where deviceTbl.DeviceToken == deviceToken && waitingListTbl.UserInfoId == userInfoId
                              && eventCollectionTbl.IsActive != false && eventCollectionTbl.IsDeleted != true
                              select waitingListTbl
                              ).FirstOrDefaultAsync();

            return waitingList;
        }
    }
}
