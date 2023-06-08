using DieNamespace;
using GameMechanics;
using Instantiated;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PartyInfoDisplayer : VisualElement
{
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<PartyInfoDisplayer> { }

    TravellerDisplayerComponent travellerDisplayerComponent;
    Label statLabel = new() { name = "StatLabel"};
    Label resourcesLabel = new() { name = "ResourcesLabel" };
    Label featuresLabel = new() { name = "FeaturesLabel" };

    public PartyInfoDisplayer() {
        styleSheets.Add(Resources.Load<StyleSheet>("CommonStyle"));

        VisualElement pictureAndStats = new() { name = "PictureAndStats" };
        
        travellerDisplayerComponent = new TravellerDisplayerComponent();
        pictureAndStats.Add(travellerDisplayerComponent);

        VisualElement statInfoBox = new() { name = "StatInformationBox" };
        VisualElement statLabelBox = new() { name = "StatLabelBox"};
        statLabelBox.Add(new Label("Stats"));
        statLabelBox.Add(statLabel);
        statLabelBox.AddToClassList("TextBackground");
        statInfoBox.Add(statLabelBox);
        VisualElement resourceLabelBox = new() { name = "ResourceLabelBox" };
        resourceLabelBox.Add(new Label("Resources"));
        resourceLabelBox.Add(resourcesLabel);
        resourceLabelBox.AddToClassList("TextBackground");
        statInfoBox.Add(resourceLabelBox);
        pictureAndStats.Add(statInfoBox);

        VisualElement featureBox = new() { name = "FeatureBox" };
        featureBox.Add(new Label("Features") { name = "Features" });
        featureBox.Add(featuresLabel);
        featureBox.AddToClassList("TextBackground");

        statLabel.AddToClassList("MediumText");
        resourcesLabel.AddToClassList("MediumText");
        featuresLabel.AddToClassList("MediumText");

        Add(pictureAndStats);
        Add(featureBox);

        SetDummyDisplayValues();
    }

    private void SetDummyDisplayValues() {
        string statText = $"Speed: {30}\nHP: {9}/{10}\n" +
            $"Hit Dice: {new Die(1,10)}\nSize: {EntitySize.Gargantuan}\n" +
            $"CreatureType: {CreatureType.Monstrosity}\n" +
            $"Strength: {10}\nDexterity: {10}\n" +
            $"Constitution: {10}\nIntelligence: {10}\n" +
            $"Wisdom: {10}\nCharisma: {10}\n";
        statLabel.text = statText;

        string resourcesText = "Resource: ASDF\n Uses: 5/10\n\nResource: Spell Slots\n Uses: 6/10";
        resourcesLabel.text = resourcesText;

        string featureText = "Sneak Attack\nBeginning at 1st level, you know how to strike " +
            "subtly and exploit a foe’s distraction. Once per turn, you can deal an extra 1d6 " +
            "damage to one creature you hit with an attack if you have advantage on the attack " +
            "roll. The attack must use a finesse or a ranged weapon. You don’t need advantage " +
            "on the attack roll if another enemy of the target is within 5 feet of it, that " +
            "enemy isn’t incapacitated, and you don’t have disadvantage on the attack roll. " +
            "The amount of the extra damage increases as you gain levels in this class, as " +
            "shown in the Sneak Attack column of the Rogue table.";
        featureText += "\n\n" + featureText;
        featuresLabel.text = featureText;
    }

    public PartyInfoDisplayer(Traveller partyMember) : this() {
        SetDisplayValues(partyMember);
    }

    public void SetDisplayValues(Traveller traveller) {
        travellerDisplayerComponent.SetSprite(traveller.travellerIdentity.spriteLibraryAsset);

        StatsSO stats = traveller.GetStats();
        string statText = $"Speed: {stats.baseSpeed}\nHP: {traveller.CurrHP}/{stats.maxHP}\n" +
            $"Hit Dice: {stats.hitDice}\nSize: {stats.size}\nCreatureType: {stats.creatureType}\n" +
            $"Strength: {stats.strength}\nDexterity: {stats.dexterity}\n" +
            $"Constitution: {stats.constitution}\nIntelligence: {stats.intelligence}\n" +
            $"Wisdom: {stats.wisdom}\nCharisma: {stats.charisma}\n";
        statLabel.text = statText;

        string resourcesText = "";
        int iterator = 1;
        int last = traveller.GetResources().resourceStatuses.Count;
        foreach (ResourceStatus resourceStatus in traveller.GetResources().resourceStatuses.Values) {
            string resourceText = $"Resource: {resourceStatus.resourceDefinition.id}\n" +
                $"Uses: {resourceStatus.remainingUses}/{resourceStatus.resourceDefinition.maxUses}";
            if (iterator < last) {
                resourceText += "\n\n";
            }
            resourcesText += resourceText;
            iterator++;
        }
        resourcesLabel.text = resourcesText;

        string featuresText = "";
        foreach (FeatureSO feature in stats.features) {
            string featureText = $"{feature.name}\n{feature.description}";
            if (feature != stats.features.Last()) {
                featureText += "\n\n";
            }
            featuresText += featureText;
        }
        featuresLabel.text = featuresText;
    }
}
