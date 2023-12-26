using Microsoft.EntityFrameworkCore;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID))]
    public class Source
    {
        public Source(Guid ID, Branch Branch, Content_Type ContentType, Guid CreatorID, Flags Flags, Format Format, DateOnly PublicationDate, string Publisher, string URL)
        {
            this.ID = ID;
            this.Branch = Branch;
            this.ContentType = ContentType;
            this.CreatorID = CreatorID;
            this.Flags = Flags;
            this.Format = Format;
            this.PublicationDate = PublicationDate;
            this.Publisher = Publisher;
            this.URL = URL;
        }

        public Source(Branch Branch, Content_Type ContentType, Guid CreatorID, Flags Flags, Format Format, DateOnly PublicationDate, string Publisher, string URL)
        {
            this.ID = new Guid();
            this.Branch = Branch;
            this.ContentType = ContentType;
            this.CreatorID = CreatorID;
            this.Flags = Flags;
            this.Format = Format;
            this.PublicationDate = PublicationDate;
            this.Publisher = Publisher;
            this.URL = URL;
        }

        public Guid ID { get; private set; }

        public Branch Branch { get; set; }

        public Content_Type ContentType { get; set; }

        public Guid CreatorID { get; set; }

        public Flags Flags { get; set; }

        public Format Format { get; set; }

        public DateOnly PublicationDate { get; set; }

        public string Publisher { get; set; }

        public string URL { get; set; }
    }

    public enum Branch
    {
        Norse = 0,
        AngloSaxon = 1,
        Continental = 2,
        PanGermanic = 4
    }

    public enum Content_Type
    {
        Reconstruction = 0,
        Philosophy = 1,
        Personal = 2,
        Historical = 4
    }

    public enum Flags
    {
        Folkist = 0,
        UndisclosedUPG = 1,
        CreatorControversy = 2
    }

    public enum Format
    {
        Paperback = 0,
        Hardcover = 1,
        Ebook = 2,
        Website = 4,
        video = 8,
        Audiobook = 16
    }
}
