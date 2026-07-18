interface StatusFlowProps {
  accountLinked: boolean;
  institutionName: string | null;
  identityVerified: boolean;
  verificationStatus: string | null;
  notificationSent: boolean;
}

function StepRow({ label, done, detail }: { label: string; done: boolean; detail?: string | null }) {
  return (
    <div style={{ display: "flex", alignItems: "center", gap: "0.75rem", padding: "0.5rem 0" }}>
      <span style={{ fontSize: "1.25rem" }}>{done ? "✅" : "⬜"}</span>
      <span style={{ fontWeight: 600 }}>{label}</span>
      {detail && <span style={{ color: "#666" }}>({detail})</span>}
    </div>
  );
}

export function StatusFlow({
  accountLinked,
  institutionName,
  identityVerified,
  verificationStatus,
  notificationSent,
}: StatusFlowProps) {
  return (
    <div style={{ marginTop: "1.5rem" }}>
      <StepRow label="Bank account linked" done={accountLinked} detail={institutionName} />
      <StepRow label="Identity verified" done={identityVerified} detail={verificationStatus} />
      <StepRow label="Confirmation email sent" done={notificationSent} />
    </div>
  );
}
