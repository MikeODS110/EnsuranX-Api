using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class TournamentStatkeeperMapper
    {
        public TournamentStatkeeper MapTournamentStatkeeperForCreate(string email,long tournamentId,int statkeeperId)
        {
            TournamentStatkeeper tournamentStatkeeper = new TournamentStatkeeper();

            tournamentStatkeeper.CreatedDateTime = DateTime.UtcNow;
            tournamentStatkeeper.LastModifiedDateTime = DateTime.UtcNow;
            tournamentStatkeeper.CreatedBy = email;
            tournamentStatkeeper.LastModifiedBy = email;
            tournamentStatkeeper.TournamentId = tournamentId;
            tournamentStatkeeper.StatkeeperId = statkeeperId;

            return tournamentStatkeeper;
        }



        public List<AllStatkeepersVenueList> MapImagesForTournamentStatkeeper(List<AllStatkeepersVenueList> allStatkeepersVenueLists,List<Images> imageList)
        {
            foreach (AllStatkeepersVenueList allStatkeepersVenue in allStatkeepersVenueLists)
            {
                var userImages = imageList.Where(x => x.TableId == allStatkeepersVenue.UserId).ToList();
                string profileImage = userImages.Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                .Select(x => x.Path).FirstOrDefault();
                string coverImage = userImages.Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())
                                                .Select(x => x.Path).FirstOrDefault();

                allStatkeepersVenue.ProfileImage = !string.IsNullOrEmpty(profileImage) ? profileImage : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                allStatkeepersVenue.CoverImage = !string.IsNullOrEmpty(coverImage) ? coverImage : Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE;
            }

            return allStatkeepersVenueLists;
        }
    }
}
