using Application.DTOs.ListingDTOs;
using Application.Interfaces.Repositories.Listings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validators.ListingValidators
{
    public class ListingCreationDTOValidator : AbstractValidator<ListingCreationDTO>
    {
        public ListingCreationDTOValidator()
        {
        }
    }
}
