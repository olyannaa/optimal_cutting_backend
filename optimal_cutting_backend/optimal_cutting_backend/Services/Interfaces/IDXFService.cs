﻿using vega.Controllers.DTO;

namespace vega.Services.Interfaces
{
    public interface IDXFService
    {
        public List<FigureDTO> GetDXF(byte[] fileBytes);
    }
}
