import { usePlaidLink } from "react-plaid-link";

interface PlaidLinkButtonProps {
  linkToken: string;
  onSuccess: (publicToken: string) => void;
}


export function PlaidLinkButton({ linkToken, onSuccess }: PlaidLinkButtonProps) {
  const { open, ready } = usePlaidLink({
    token: linkToken,
    onSuccess: (publicToken) => onSuccess(publicToken),
  });

  return (
    <button onClick={() => open()} disabled={!ready}>
      Connect a bank account
    </button>
  );
}
