type PropertyCardProps = {
  title: string;
  location: string;
  price: string;
  status: "published" | "draft";
};

export function PropertyCard({ title, location, price, status }: PropertyCardProps) {
  return (
    <article className="property-card">
      <div className="property-thumb" aria-hidden="true" />
      <div className="property-card-body">
        <div className="badge-row">
          <span className={status === "published" ? "badge published" : "badge draft"}>
            {status === "published" ? "منشور" : "غير منشور"}
          </span>
        </div>
        <h3>{title}</h3>
        <p>{location}</p>
        <strong>{price}</strong>
      </div>
    </article>
  );
}
