using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class PlayerInterestActivityRepository : IPlayerInterestActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public PlayerInterestActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerInterestActivity>> GetPlayerInterestsAcititiesById(long userId)
        {
            return await _context.PlayerInterestActivity.AsNoTracking().Where(x => x.UserInfoId == userId).Include(x => x.ActivityCategory).ToListAsync();
        }

        public async Task<bool> DeleteUserInterest(long userId)
        {
            List<PlayerInterestActivity> playerInterestActivityList = await _context.PlayerInterestActivity.Where(x => x.UserInfoId == userId).ToListAsync();

            _context.PlayerInterestActivity.RemoveRange(playerInterestActivityList);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MemberDetail>> GetPlayerSuggestionBasedOnInterestMatch(List<long> activityCategoryIdList,long userId, BasicFilter basicFilter, IQueryable<Follow> followerList, long loginUserId)
        {
            var query = await (from playerInterestActivityTbl in _context.PlayerInterestActivity.AsNoTracking()
                               join userInfoTbl in _context.UserInfo.AsNoTracking()
                               on playerInterestActivityTbl.UserInfoId equals userInfoTbl.Id

                               join userProfileImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                               on userInfoTbl.Id equals userProfileImage.TableId into newUserProfileImage
                               from userFullProfileImage in newUserProfileImage.DefaultIfEmpty()

                               join userCoverImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())
                               on userInfoTbl.Id equals userCoverImage.TableId into newUserCoverImage
                               from userFullCoverImage in newUserCoverImage.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId)
                               on userInfoTbl.Id equals followTbl.FolloweeId into newFullFollowee
                               from ifThisUserFollowTheseUser in newFullFollowee.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId)
                               on userInfoTbl.Id equals followTbl.FollowerId into newFullFolloweeExt
                               from ifTheUserIsFollowedByTheseUser in newFullFolloweeExt.DefaultIfEmpty()

                               where activityCategoryIdList.Contains(playerInterestActivityTbl.ActivityCategoryId)
                               where userInfoTbl.Id != userId && !followerList.Select(x => x.FolloweeId).Contains(userInfoTbl.Id) && userInfoTbl.Id != loginUserId
                               where userFullProfileImage != null ? userFullProfileImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : userInfoTbl.Id != 0
                               where userFullCoverImage != null ? userFullCoverImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : userInfoTbl.Id != 0

                               select new
                               {
                                   userInfo = userInfoTbl,
                                   userFullCoverImage = userFullCoverImage,
                                   userFullProfileImage = userFullProfileImage,
                                   ifThisUserFollowTheseUser = ifThisUserFollowTheseUser,
                                   ifTheUserIsFollowedByTheseUser = ifTheUserIsFollowedByTheseUser
                               }).GroupBy(x => x.userInfo.Id).Select(x => new MemberDetail {
                                    Id = x.Key,
                                    Name = x.First().userInfo.Name,
                                    ProfileImage = x.First().userFullProfileImage != null ? x.First().userFullProfileImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                    CoverImage = x.First().userFullProfileImage != null ? x.First().userFullProfileImage.Path : Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE,
                                   Follow = x.First().ifThisUserFollowTheseUser != null ? x.First().ifThisUserFollowTheseUser.Status == 1 ? Follows.Following : Follows.RequestPending : Follows.RequestToFollow
                                   //x.First().ifTheUserIsFollowedByTheseUser != null ? x.First().ifTheUserIsFollowedByTheseUser.Status == 1 ? Follows.Following : Follows.RequestPending : Follows.RequestToFollow
                               }).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();
            return query;
        }
        
        public async Task<int> GetPlayerSuggestionBasedOnInterestMatchCount(List<long> activityCategoryIdList,long userId, IQueryable<Follow> followerList,long loginUserId)
        {
            var query = await (from playerInterestActivityTbl in _context.PlayerInterestActivity.AsNoTracking()
                               join userInfoTbl in _context.UserInfo.AsNoTracking()
                               on playerInterestActivityTbl.UserInfoId equals userInfoTbl.Id

                               join userProfileImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                               on userInfoTbl.Id equals userProfileImage.TableId into newUserProfileImage
                               from userFullProfileImage in newUserProfileImage.DefaultIfEmpty()

                               join userCoverImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())
                               on userInfoTbl.Id equals userCoverImage.TableId into newUserCoverImage
                               from userFullCoverImage in newUserCoverImage.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId)
                               on userInfoTbl.Id equals followTbl.FolloweeId into newFullFollowee
                               from ifThisUserFollowTheseUser in newFullFollowee.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId)
                               on userInfoTbl.Id equals followTbl.FollowerId into newFullFolloweeExt
                               from ifTheUserIsFollowedByTheseUser in newFullFolloweeExt.DefaultIfEmpty()

                               where activityCategoryIdList.Contains(playerInterestActivityTbl.ActivityCategoryId)
                               where userInfoTbl.Id != userId && !followerList.Select(x => x.FolloweeId).Contains(userInfoTbl.Id) && userInfoTbl.Id != loginUserId
                               where userFullProfileImage != null ? userFullProfileImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : userInfoTbl.Id != 0
                               where userFullCoverImage != null ? userFullCoverImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : userInfoTbl.Id != 0

                               select new
                               {
                                   userInfo = userInfoTbl,
                                   userFullCoverImage = userFullCoverImage,
                                   userFullProfileImage = userFullProfileImage,
                                   ifThisUserFollowTheseUser = ifThisUserFollowTheseUser,
                                   ifTheUserIsFollowedByTheseUser = ifTheUserIsFollowedByTheseUser
                               }).GroupBy(x => x.userInfo.Id).Select(x => new MemberDetail {
                                    Id = x.Key,
                               }).CountAsync();
            return query;
        }
    }
}
