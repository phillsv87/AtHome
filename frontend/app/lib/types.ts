export interface StreamSession
{

    Id: string;

    StreamId: number;

    StreamName: string;

    Token: string;

    Expirers: Date | string;

    TTLSeconds: number;

    Uri: string;
}

export interface StreamInfo
{
    Id: number;

    Name: string;
}