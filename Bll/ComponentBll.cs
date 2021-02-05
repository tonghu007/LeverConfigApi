using Lever.Dal;
using Lever.IBLL;

namespace Lever.Bll
{
    public class ComponentBll: IComponentBll
    {
        private readonly ComponentDal _dal;
        public ComponentBll(ComponentDal dal)
        {
            _dal = dal;
        }
    }
}
