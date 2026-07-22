using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TrainingModule.Controllers;

[Route("api/training-arrival-appointment")]
public class TrainingArrivalAppointmentController(ITrainingArrivalAppointmentService service)
    : CrudController<TrnArrivalAppointment>(service);
