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
    ReceiveNotifications:boolean;
}

export interface ApiInfo
{
    Id: string;

    Version: string;
}

export enum NotificationType {
    Other = 0,
    APN = 1,
}

export interface NotificationDevice
{
    Id: string;
    Type: NotificationType;
}