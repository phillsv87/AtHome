import HsConfig from "./HsConfig";
import History from "../CommonJs/History-rn";
import Http from "../CommonJs/Http";

export default class HsApp
{

    public readonly app:HsApp;

    public readonly config:HsConfig;

    public readonly history:History;

    public readonly http:Http;

    public clientToken:string|null='TGhHZ3pVd3N2ZU5abjVUSnhTZE1IcU9vcDM5RG1ZWA';

    constructor(config:HsConfig)
    {
        this.app=this;
        this.config=config;
        this.history=new History();
        this.http=new Http(config.ApiBaseUrl,true);
    }

    public async initAsync():Promise<boolean>
    {
        return true;
    }

    public dispose()
    {

    }
}