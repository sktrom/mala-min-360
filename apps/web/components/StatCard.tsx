type StatCardProps = {
  label: string;
  value: string;
};

export function StatCard({ label, value }: StatCardProps) {
  return (
    <article className="stat-card">
      <div className="stat-value">{value}</div>
      <p className="stat-label">{label}</p>
    </article>
  );
}
