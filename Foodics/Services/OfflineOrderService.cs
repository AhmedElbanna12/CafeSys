using Foodics.Dtos.Admin.Orders;
using Newtonsoft.Json;

namespace Foodics.Services
{
    public class OfflineOrderService
    {
        private readonly string _filePath = "offline_orders.json";

        public List<OfflineOrderDto> GetOfflineOrders()
        {
            if (!File.Exists(_filePath))
                return new List<OfflineOrderDto>();

            var json = File.ReadAllText(_filePath);

            return JsonConvert.DeserializeObject<List<OfflineOrderDto>>(json)
                   ?? new List<OfflineOrderDto>();
        }

        public void SaveOfflineOrder(OfflineOrderDto order)
        {
            var orders = GetOfflineOrders();

            order.OfflineOrderId = orders.Count == 0
                ? 1
                : orders.Max(o => o.OfflineOrderId) + 1;

            order.IsSynced = false;

            orders.Add(order);

            File.WriteAllText(_filePath,
                JsonConvert.SerializeObject(orders, Formatting.Indented));
        }

        public void DeleteOfflineOrders(List<Object> syncedIds)
        {
            var orders = GetOfflineOrders();

            var remainingOrders = orders
                .Where(o => !syncedIds.Contains(o.OfflineOrderId))
                .ToList();

            File.WriteAllText(_filePath,
                JsonConvert.SerializeObject(remainingOrders, Formatting.Indented));
        }

        public void DeleteAllOfflineOrders()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
    }
}