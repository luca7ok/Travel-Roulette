using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliHack18.Services
{
    public class NavMenuHide
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsUserLoggedIn => !string.IsNullOrWhiteSpace(UserId);
        public void SetUserId(string id)
        {
            UserId = id;
            NotifyStateChanged();
        }
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

    }
}
