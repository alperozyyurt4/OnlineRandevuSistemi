namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class CustomerDashboardViewModel
    {

        public List<PopularServiceViewModel> PopularServices { get; set; }
    }
    public class PopularServiceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AppointmentCount { get; set; }
    }
}
