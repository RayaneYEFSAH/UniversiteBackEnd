using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteDomainUnitTest;

public class ParcoursUeUnitTest
{
    [Test]
    public async Task AddUeDansParcours_ShouldWork()
    {
        // 1. DATA
        long idParcours = 1;
        long idUe = 5;
        
        // On prépare un parcours avec une liste d'UEs initialisée (mais vide)
        var parcours = new Parcours { Id = idParcours, NomParcours = "Master 1", UesEnseignees = new List<Ue>() };
        var ue = new Ue { Id = idUe, Intitule = "Java", NumeroUe = "UE01" };

        // 2. MOCKS
        var mockParcoursRepo = new Mock<IParcoursRepository>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        // Quand on cherche le parcours, on le trouve
        mockParcoursRepo.Setup(r => r.FindAsync(idParcours)).ReturnsAsync(parcours);
        // Quand on cherche l'UE, on la trouve
        mockUeRepo.Setup(r => r.FindAsync(idUe)).ReturnsAsync(ue);
        
        // Configuration de la Factory
        mockFactory.Setup(f => f.ParcoursRepository()).Returns(mockParcoursRepo.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);

        // 3. EXECUTION
        var useCase = new AddUeDansParcoursUseCase(mockFactory.Object);
        var result = await useCase.ExecuteAsync(idParcours, idUe);

        // 4. VERIFICATION
        // On vérifie que le parcours retourné contient bien l'UE
        Assert.That(result.UesEnseignees, Does.Contain(ue));
        // On vérifie que SaveChanges a été appelé
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}