import React from 'react';
import { useApp } from '../lib/hooks';
import { useAsync } from '../CommonJs/AsyncHooks';
import Log from '../CommonJs/Log';
import { StreamInfo } from '../lib/types';
import { View, Button } from 'react-native';
import Busy from './Busy';
import ListItem from './ListItem';

interface StreamListProps
{
    locationId?:string;
}

export default function StreamList({
    locationId
}:StreamListProps){

    const {http,config,history,locations}=useApp();

    const location=locations.getLocation(locationId);

    const streams=useAsync(null,async ()=>{

        if(!location){
            return null;
        }

        try{

            return await http.getAsync<StreamInfo[]>(`${location.ApiBaseUrl}api/Stream`,{ClientToken:location.Token});

        }catch(ex){
            Log.error('Unable to load stream list',ex);
        }

    },[http,config,location]);

    if(!streams || !location){
        return <Busy/>
    }

    return (
        <View>
            {streams.map((s)=>(
                <ListItem title={s.Name} key={s.Id} onPress={()=>history.push(`/location/${location.Id}/stream/${s.Id}`)} />
            ))}
            <Button title="Config" onPress={()=>history.push(`/location/${location.Id}/config`)}/>
        </View>
    )

}
