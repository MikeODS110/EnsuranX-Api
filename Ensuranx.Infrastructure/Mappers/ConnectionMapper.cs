using Ensuranx.Application.Requests.Branch;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class ConnectionMapper
    {
     


        public List<Connection> MapConnectionListForUpdateBranch(List<Connection> connectionList, UpdateBranch updateBranch)
        {
            foreach (Connection connection in connectionList)
            {
                switch (connection.ConnectionTypeId)
                {
                    case (long)Domain.Enums.Enum.ConnectionType.Twitter:
                        connection.Value = string.IsNullOrEmpty(updateBranch.Twitter) != true ? updateBranch.Twitter : connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Email:
                        connection.Value = string.IsNullOrEmpty(updateBranch.Email) != true ?  updateBranch.Email : connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Website:
                        connection.Value = string.IsNullOrEmpty(updateBranch.Website) != true ? updateBranch.Website : connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Facebook:
                        connection.Value = string.IsNullOrEmpty(updateBranch.Facebook) != true ? updateBranch.Facebook : connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.PhoneNumber:
                        connection.Value = string.IsNullOrEmpty(updateBranch.PhoneNumber) != true ? updateBranch.PhoneNumber : connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Address:
                        connection.Value = string.IsNullOrEmpty(updateBranch.Address) != true ? updateBranch.Address : connection.Value;
                        break;
                    default:
                        break;
                }
            }

            return connectionList;
        }
    }
}
