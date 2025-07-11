namespace Sotex.EDSPortal.Shared.DTOs
{
        public class IntegrationRequestV2Dto
        {
            public string RequestRef { get; set; }
            public int OfficialId { get; set; } // ?
            public bool IsTaxPaid { get; set; } // ?
            public int TaxAmount { get; set; } // ?
            public ClientV2Dto? Client { get; set; }
            public BuildingV2Dto? Building { get; set; }
            public List<ContactPersonV2Dto>? ContactPersons { get; set; }
            public SubjectV2Dto? Subject { get; set; }
        }

        public class ClientV2Dto
        {
            public bool? IsCompany { get; set; }
            public AddressV2Dto? ClientAddress { get; set; }
            public List<string>? Phones { get; set; }
            public List<string>? Emails { get; set; }
            public string? LegalName { get; set; }
            public string? RepresentativeFirstName { get; set; }
            public string? RepresentativeLastName { get; set; }
            public string? RegNumber { get; set; }
            public string? TaxNumber { get; set; }
            public string? Account { get; set; }
            public string? PersonFirstName { get; set; }
            public string? PersonLastName { get; set; }
            public string? PersonRegNumber { get; set; }
        }

        public class AddressV2Dto
        {
            public int SettlementId { get; set; }
            public long StreetId { get; set; }
            public string? Street { get; set; }
            public string? HouseNo { get; set; }
            public int ZipCode { get; set; }
        }

        public class BuildingV2Dto
        {
            public AddressV2Dto BuildingAddress { get; set; }
            public string? LocationDescription { get; set; }
            public List<string> BuildingType { get; set; }
            public List<string> HeatingType { get; set; }
            public List<string> BuildingPurpose { get; set; }
            public bool IsExistingInstallation { get; set; }
            public int DeadlineMonth { get; set; }
            public int DeadlineYear { get; set; }
            public bool IsConnected { get; set; }
            public List<string> ConnectionCodes { get; set; }
            public List<CadastralParcelV2Dto> CadastralParcels { get; set; } 
            public List<PreviousApprovalV2Dto> PreviousApprovals { get; set; }
        }

        public class CadastralParcelV2Dto
        {
            public int CadastralMunicipalityId { get; set; }
            public string CadastralParcelCode { get; set; } 
        }

        public class PreviousApprovalV2Dto
        {
            public string PreviousNumber { get; set; }
            public DateTime PreviousDate { get; set; }
        }

        public class ContactPersonV2Dto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public AddressV2Dto ContactAddress { get; set; }
            public List<string> Phones { get; set; }
            public List<string> Emails { get; set; }
            public bool IsDefault { get; set; }
        }

        public class SubjectV2Dto
        {
            public bool? IsGroupConnection { get; set; }
            public List<string>? ExistingInstalations { get; set; }
            public string? VoltageId { get; set; }
            public string ConnectionDurationId { get; set; } // ? 
            public DurationV2Dto? Duration { get; set; } 
            public string? TechnologyDescription { get; set; }
            public bool? IsResigningPrevious { get; set; } // SALJEMO NULL
            public string? ResignPreviousNumber { get; set; } // SALJEMO NULL
            public DateTime? ResignPreviousDate { get; set; } // SALJEMO NULL
            public string? MeterTypeId { get; set; }
            public string? TariffNumberId { get; set; }
            public string? PowerCode { get; set; } 
            public decimal? PowerRequested { get; set; }
            public int? FuseCurrent { get; set; } 
            public List<RequestConnectionSpecificationV2Dto>? RequestConnectionSpecifications { get; set; }
        }

        public class DurationV2Dto
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class RequestConnectionSpecificationV2Dto
        {
            public int OrdinalNumber { get; set; }
            public string BuildingUnitPurpose { get; set; }
            public int Quantity { get; set; }
            public string PowerCode { get; set; }
            public decimal Power { get; set; }
            public int FuseCurrent { get; set; }
            public string MeterTypeId { get; set; }
            public string TariffNumberId { get; set; }
        }
}
