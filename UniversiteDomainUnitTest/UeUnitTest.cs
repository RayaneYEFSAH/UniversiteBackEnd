using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateUeUseCase_Success()
    {
        // 1. ARRANGE (Préparation)
        long idGenere = 10;
        string numUe = "INFO01";
        string intitule = "Programmation C#";

        var ueAvant = new Ue { NumeroUe = numUe, Intitule = intitule };
        var ueApres = new Ue { Id = idGenere, NumeroUe = numUe, Intitule = intitule };

        // Création des Mocks
        var mockUeRepo = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        // Simulation : Aucune UE existante avec ce numéro (retourne liste vide)
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // Simulation : La création renvoie l'UE avec un ID
        mockUeRepo
            .Setup(repo => repo.CreateAsync(It.IsAny<Ue>()))
            .ReturnsAsync(ueApres);

        // Configuration de la Factory pour renvoyer notre mock de repo
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);

        // Création du UseCase
        var useCase = new CreateUeUseCase(mockFactory.Object);

        // 2. ACT (Action)
        var resultat = await useCase.ExecuteAsync(ueAvant);

        // 3. ASSERT (Vérification)
        Assert.That(resultat.Id, Is.EqualTo(idGenere));
        Assert.That(resultat.NumeroUe, Is.EqualTo(numUe));
        Assert.That(resultat.Intitule, Is.EqualTo(intitule));
        
        // Vérifie que SaveChangesAsync a bien été appelé une fois
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateUeUseCase_DuplicateNumber_ThrowsException()
    {
        // ARRANGE
        string numUe = "INFO01";
        string intitule = "Algorithmique";
        var ue = new Ue { NumeroUe = numUe, Intitule = intitule };

        var mockUeRepo = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        // Simulation : Une UE existe déjà ! (La liste n'est pas vide)
        var listeExistante = new List<Ue> { new Ue { Id = 5, NumeroUe = numUe } };
        
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(listeExistante);

        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);

        var useCase = new CreateUeUseCase(mockFactory.Object);

        // ACT & ASSERT
        // On vérifie que l'appel lève bien l'exception DuplicateUeException
        Assert.ThrowsAsync<DuplicateUeException>(async () => await useCase.ExecuteAsync(ue));
    }

    [Test]
    public void CreateUeUseCase_ShortName_ThrowsException()
    {
        // ARRANGE
        // Intitulé trop court (<= 3 caractères)
        var ue = new Ue { NumeroUe = "INFO02", Intitule = "C#" };

        var mockFactory = new Mock<IRepositoryFactory>(); 
        // Pas besoin de mocker le repo ici car l'exception est levée avant l'appel à la BD

        var useCase = new CreateUeUseCase(mockFactory.Object);

        // ACT & ASSERT
        Assert.ThrowsAsync<InvalidNomUeException>(async () => await useCase.ExecuteAsync(ue));
    }
}