using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class Gene
    {
        private bool isGene;
        private string geneType;

        #region Properties
        public string GeneType
        {
            get { return geneType; }
            set { geneType = value; }
        }
        #endregion

        public Gene()
        {
            geneType = "";
            isGene = false;
        }

        public Gene(String sourceCode)
        {
            geneType = "";
            isGene = true;

            //For custom CDS: will not have source code needed for scraping, so end here
            if (sourceCode.Equals("custom")) return;

            //reporter types
            if (sourceCode.Contains("//function/reporter/fluorescence")) geneType = "Reporter: Fluorescent protein";
            if (sourceCode.Contains("//function/reporter/light")) geneType = "Reporter: Luciferase";
            if (sourceCode.Contains("//function/reporter/color")) geneType = "Reporter: Enzyme that produce colored substrates";
            if (sourceCode.Contains("//function/reporter/")) geneType = "Reporter";
            //miscellaneous reporter types --> what do you want the geneType to be?

            //transcriptional regulator types
            if (sourceCode.Contains("//cds/transcriptionalregulator/activator") && sourceCode.Contains("//cds/transcriptionalregulator/repressor")) geneType = "Transcriptional Regulator: Multiple";
            if (sourceCode.Contains("//cds/transcriptionalregulator/repressor")) geneType = "Transcriptional Regulator: Repressor";
            if (sourceCode.Contains("//cds/transcriptionalregulator/activator")) geneType = "Transcriptional Regulator: Activator";

            //selection marker
            if (sourceCode.Contains("//cds/selectionmarker")) geneType = "Selection marker";

            //biosynthesis and degradation types
            //- biosynthesis and degradation of AHL 
            if (sourceCode.Contains("//function/biosynthesis/ahl ")) geneType = "Biosynthesis of AHL";
            else if (sourceCode.Contains("//function/degradation/ahl")) geneType = "Degradation of AHL";
            else if (sourceCode.Contains("//function/degradation/ahl") && sourceCode.Contains("//function/biosynthesis/ahl")) geneType = "Biosynthesis and Degradation of AHL";
            //- biosynthesis of isoprenoids
            else if (sourceCode.Contains("//function/biosynthesis/isoprenoid")) geneType = "Biosynthesis of isoprenoid";
            //- Biosynthesis of odorants 
            else if (sourceCode.Contains("//function/biosynthesis/odorant")) geneType = "Biosynthesis of odorant";
            //- biosynthesis of plastic
            else if (sourceCode.Contains("//function/biosynthesis/plastic")) geneType = "Biosynthesis of plastic";
            //- biosynthesis of butanol
            else if (sourceCode.Contains("//function/biosynthesis/butanol")) geneType = "Biosynthesis of butanol";
            //- degradation of bisphenol A
            else if (sourceCode.Contains("//function/degradation/bisphenol")) geneType = "Degradation of bisphenol A";
            //- degradation of cellulose
            else if (sourceCode.Contains("//function/degradation/cellulose")) geneType = "Degradation of cellulose";
            //!!!!!!!!!!!!!!- misc. --> how do you want to enter it into the instance variable?
            else if (sourceCode.Contains("//function/biosynthesis\n")) geneType = "Biosynthesis";
            else if (sourceCode.Contains("//function/degradation\n")) geneType = "Degradation";
            

            //DNA modification type
            //- Restriction enzymes
            if (sourceCode.Contains("//cds/enzyme/endonuclease/restriction")) geneType = "DNA modification: Restriction enzyme";
            //- DNA ligases
            if (sourceCode.Contains("//cds/enzyme/ligase")) geneType = "DNA modification: DNA ligase";
            //- Recombinases
            if (sourceCode.Contains("//function/recombination")) geneType = "DNA modification: Recombinase";
            //- Chromatin proteins
            if (sourceCode.Contains("//cds/chromatinremodeling")) geneType = "DNA modificatoin: Chromatin protein";

            //Proteases type
            if (sourceCode.Contains("//cds/enzyme/protease")) geneType = "Protease";

            //Post-translational modification enzymes type
            //- Phosphorylation
            if (sourceCode.Contains("//cds/enzyme/phosphorylation")) geneType = "Post-translational modification enzyme: Phosphorylation";
            //- Methylation
            if (sourceCode.Contains("//cds/enzyme/methylation")) geneType = "Post-translational modification enzyme: Methylation";

            //Membrane proteins
            //- Receptors
            if (sourceCode.Contains("//cds/membrane/receptor")) geneType = "Membrane protein: Receptor";
            //- Transporters
            if (sourceCode.Contains("//cds/membrane/transporter")) geneType = "Membrane protein: Transporter";
            //- Channels
            if (sourceCode.Contains("//cds/membrane/channel")) geneType = "Membrane protein: Channel";
            //- Pumps
            if (sourceCode.Contains("//cds/membrane/pump")) geneType = "Membrane protein: Pump";
            //- Other membrane proteins --> How to enter as geneType?
            if (sourceCode.Contains("//cds/membrane")) geneType = "Membrane protein";

            //Receptors and Ligands
            //- Receptors
            if (sourceCode.Contains("//cds/receptor")) geneType = "Receptor";
            //- Ligands
            if (sourceCode.Contains("//cds/ligand")) geneType = "Ligand";

            //Lysis proteins
            if (sourceCode.Contains("/lysis")) geneType = "Lysis protein";


        }
    }
}
