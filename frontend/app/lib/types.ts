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


export interface Location
{
    Id:string;
    Name:string;
    ApiBaseUrl:string;
    Token:string;
}

export interface ApiInfo
{
    Id: string;

    Version: string;
}