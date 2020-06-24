import HsApp from "./HsApp";
import { Location, ApiInfo } from "./types";
import Log from "../CommonJs/Log";
import { aryRemoveItem } from "../CommonJs/commonUtils";

const storeKey="LocationManager";

export default class LocationManager
{

    private readonly app:HsApp;

    private _locations:ReadonlyArray<Location>=[];
    public get locations(){return this._locations}

    constructor(app:HsApp)
    {
        this.app=app;
    }

    public async initAsync()
    {
        try{

            this._locations=await this.app.store.loadOrDefaultAsync<Location[]>(storeKey,[]);

        }catch(ex){
            Log.error('Unable to load locations',ex);
        }
    }

    public async addLocationAsync(name:string,apiBaseUrl:string,token:string):Promise<Location>
    {

        if(!apiBaseUrl.endsWith('/')){
            apiBaseUrl+='/';
        }

        let info:ApiInfo;
        
        try{
            info=await this.app.http.getAsync<ApiInfo>(apiBaseUrl+'api/Info');
        }catch{
            throw new Error('Invalid URL')
        }

        try{
            //test token
            await this.app.http.getAsync(`${apiBaseUrl}api/Stream`,{clientToken:token});
        }catch{
            throw new Error('Invalid Token');
        }

        const location:Location={
            Id:info.Id,
            Name:name,
            ApiBaseUrl:apiBaseUrl,
            Token:token
        }


        this._locations=[...this._locations,location];

        await this.app.store.saveAsync(storeKey,this._locations);

        return location;

    }

    public async deleteLocationAsync(id:string):Promise<boolean>
    {
        const location=this.getLocation(id);
        if(!location){
            return false;
        }

        const nl=[...this._locations];
        aryRemoveItem(nl,location);
        this._locations=nl;

        await this.app.store.saveAsync(storeKey,this._locations);
        
        return true;
    }

    public getLocation(id:string|null|undefined):Location|null
    {
        return this._locations.find(l=>l.Id===id)||null;
    }
}