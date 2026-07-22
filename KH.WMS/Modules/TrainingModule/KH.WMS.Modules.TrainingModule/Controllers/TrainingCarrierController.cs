using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TrainingModule.Controllers;

[Route("api/training-carrier")]
public class TrainingCarrierController(ITrainingCarrierService service) : CrudController<TrnCarrier>(service);
